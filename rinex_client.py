"""
RINEX Client - Main module for downloading, parsing and storing RINEX files
"""
import os
import sys
import gzip
import hashlib
import logging
import argparse
import requests
from datetime import datetime, timedelta
from pathlib import Path
from typing import List, Dict, Any, Optional
from urllib.parse import urljoin

from config import Config
from database import DatabaseManager
from rinex_parser import RinexParser

class RinexClient:
    """Main client for RINEX file operations"""
    
    def __init__(self, config: Config = None):
        """Initialize RINEX client"""
        self.config = config or Config()
        self.db_manager = DatabaseManager()
        self.parser = RinexParser()
        self.session = requests.Session()
        
        # Setup download directory
        self.download_dir = Path(self.config.DOWNLOAD_DIR)
        self.download_dir.mkdir(exist_ok=True)
        
        # Configure requests session
        self.session.headers.update({
            'User-Agent': 'RINEX-Client/1.0'
        })
        
        self._setup_logging()
        
    def _setup_logging(self):
        """Setup logging configuration"""
        logging.basicConfig(
            level=getattr(logging, self.config.LOG_LEVEL),
            format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
            handlers=[
                logging.FileHandler(self.config.LOG_FILE),
                logging.StreamHandler(sys.stdout)
            ]
        )
        self.logger = logging.getLogger(__name__)
        
    def download_file(self, url: str, filename: str) -> Optional[str]:
        """Download RINEX file from URL"""
        file_path = self.download_dir / filename
        
        # Check if file already exists
        if file_path.exists():
            self.logger.info(f"File {filename} already exists, skipping download")
            return str(file_path)
            
        self.logger.info(f"Downloading {filename} from {url}")
        
        try:
            response = self.session.get(
                url, 
                timeout=self.config.TIMEOUT,
                stream=True
            )
            response.raise_for_status()
            
            # Download file in chunks
            with open(file_path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        
            file_size = file_path.stat().st_size
            self.logger.info(f"Downloaded {filename} ({file_size} bytes)")
            return str(file_path)
            
        except requests.RequestException as e:
            self.logger.error(f"Error downloading {filename}: {e}")
            if file_path.exists():
                file_path.unlink()  # Remove partial file
            return None
        except Exception as e:
            self.logger.error(f"Unexpected error downloading {filename}: {e}")
            if file_path.exists():
                file_path.unlink()
            return None
            
    def calculate_checksum(self, file_path: str) -> str:
        """Calculate MD5 checksum of file"""
        try:
            hash_md5 = hashlib.md5()
            with open(file_path, "rb") as f:
                for chunk in iter(lambda: f.read(4096), b""):
                    hash_md5.update(chunk)
            return hash_md5.hexdigest()
        except Exception as e:
            self.logger.error(f"Error calculating checksum for {file_path}: {e}")
            return ""
            
    def process_date(self, date: datetime, file_types: List[str] = None) -> Dict[str, Any]:
        """Process RINEX files for a specific date"""
        if file_types is None:
            file_types = ['broadcast']
            
        results = {
            'date': date,
            'processed_files': [],
            'failed_files': [],
            'navigation_records': 0
        }
        
        self.logger.info(f"Processing RINEX files for {date.strftime('%Y-%m-%d')}")
        
        for file_type in file_types:
            try:
                # Generate download URL
                url = self.config.get_download_url(date, file_type)
                filename = os.path.basename(url)
                
                # Check if file already processed
                if self.db_manager.file_exists(filename):
                    self.logger.info(f"File {filename} already in database, skipping")
                    continue
                    
                # Download file with retries
                file_path = None
                for attempt in range(self.config.MAX_RETRIES):
                    file_path = self.download_file(url, filename)
                    if file_path:
                        break
                    if attempt < self.config.MAX_RETRIES - 1:
                        self.logger.warning(f"Download attempt {attempt + 1} failed, retrying...")
                        
                if not file_path:
                    results['failed_files'].append(filename)
                    continue
                    
                # Calculate file info
                file_size = Path(file_path).stat().st_size
                checksum = self.calculate_checksum(file_path)
                
                # Add file to database
                file_id = self.db_manager.add_rinex_file(
                    filename=filename,
                    file_date=date,
                    file_type=file_type,
                    file_size=file_size,
                    file_path=file_path,
                    checksum=checksum
                )
                
                # Parse and store navigation data
                try:
                    parsed_data = self.parser.parse_file(file_path)
                    nav_data = parsed_data.get('navigation_data', [])
                    
                    if nav_data:
                        # Validate data
                        valid_nav_data = self.parser.validate_data(nav_data)
                        
                        # Prepare data for database
                        db_nav_data = []
                        for record in valid_nav_data:
                            record['rinex_file_id'] = file_id
                            db_nav_data.append(record)
                            
                        # Store in database
                        if db_nav_data:
                            records_added = self.db_manager.add_navigation_data(db_nav_data)
                            results['navigation_records'] += records_added
                            
                    # Update file status
                    self.db_manager.update_file_status(file_id, 'processed')
                    results['processed_files'].append(filename)
                    
                    self.logger.info(f"Successfully processed {filename} - {len(nav_data)} records")
                    
                except Exception as e:
                    self.logger.error(f"Error parsing {filename}: {e}")
                    self.db_manager.update_file_status(file_id, 'error')
                    results['failed_files'].append(filename)
                    
            except Exception as e:
                self.logger.error(f"Error processing {file_type} for {date}: {e}")
                results['failed_files'].append(f"{file_type}_{date.strftime('%Y%m%d')}")
                
        return results
        
    def process_date_range(self, start_date: str = None, end_date: str = None,
                          file_types: List[str] = None) -> Dict[str, Any]:
        """Process RINEX files for a date range"""
        dates = self.config.get_date_range(start_date, end_date)
        
        overall_results = {
            'start_date': dates[0] if dates else None,
            'end_date': dates[-1] if dates else None,
            'total_dates': len(dates),
            'processed_files': [],
            'failed_files': [],
            'total_navigation_records': 0,
            'date_results': []
        }
        
        for date in dates:
            try:
                date_result = self.process_date(date, file_types)
                overall_results['date_results'].append(date_result)
                overall_results['processed_files'].extend(date_result['processed_files'])
                overall_results['failed_files'].extend(date_result['failed_files'])
                overall_results['total_navigation_records'] += date_result['navigation_records']
                
            except Exception as e:
                self.logger.error(f"Error processing date {date}: {e}")
                overall_results['failed_files'].append(f"date_{date.strftime('%Y%m%d')}")
                
        return overall_results
        
    def reprocess_failed_files(self) -> Dict[str, Any]:
        """Reprocess files that failed previously"""
        unprocessed_files = self.db_manager.get_unprocessed_files()
        
        results = {
            'total_files': len(unprocessed_files),
            'processed_files': [],
            'failed_files': [],
            'navigation_records': 0
        }
        
        self.logger.info(f"Reprocessing {len(unprocessed_files)} failed files")
        
        for rinex_file in unprocessed_files:
            try:
                if not os.path.exists(rinex_file.file_path):
                    self.logger.warning(f"File not found: {rinex_file.file_path}")
                    self.db_manager.update_file_status(rinex_file.id, 'error')
                    results['failed_files'].append(rinex_file.filename)
                    continue
                    
                # Parse file
                parsed_data = self.parser.parse_file(rinex_file.file_path)
                nav_data = parsed_data.get('navigation_data', [])
                
                if nav_data:
                    # Validate data
                    valid_nav_data = self.parser.validate_data(nav_data)
                    
                    # Prepare data for database
                    db_nav_data = []
                    for record in valid_nav_data:
                        record['rinex_file_id'] = rinex_file.id
                        db_nav_data.append(record)
                        
                    # Store in database
                    if db_nav_data:
                        records_added = self.db_manager.add_navigation_data(db_nav_data)
                        results['navigation_records'] += records_added
                        
                # Update file status
                self.db_manager.update_file_status(rinex_file.id, 'processed')
                results['processed_files'].append(rinex_file.filename)
                
                self.logger.info(f"Successfully reprocessed {rinex_file.filename}")
                
            except Exception as e:
                self.logger.error(f"Error reprocessing {rinex_file.filename}: {e}")
                self.db_manager.update_file_status(rinex_file.id, 'error')
                results['failed_files'].append(rinex_file.filename)
                
        return results
        
    def get_statistics(self) -> Dict[str, Any]:
        """Get processing statistics"""
        with self.db_manager.get_session() as session:
            from database import RinexFile, NavigationData
            
            total_files = session.query(RinexFile).count()
            processed_files = session.query(RinexFile).filter(RinexFile.processed == 'processed').count()
            failed_files = session.query(RinexFile).filter(RinexFile.processed == 'error').count()
            pending_files = session.query(RinexFile).filter(RinexFile.processed == 'pending').count()
            
            total_nav_records = session.query(NavigationData).count()
            
            # Get records by satellite system
            systems = {}
            for system in ['GPS', 'GLONASS', 'Galileo', 'BeiDou']:
                count = session.query(NavigationData).filter(
                    NavigationData.satellite_system == system
                ).count()
                systems[system] = count
                
            return {
                'files': {
                    'total': total_files,
                    'processed': processed_files,
                    'failed': failed_files,
                    'pending': pending_files
                },
                'navigation_records': {
                    'total': total_nav_records,
                    'by_system': systems
                }
            }


def main():
    """Main entry point for command-line usage"""
    parser = argparse.ArgumentParser(description='RINEX Client for GNSS data download and processing')
    parser.add_argument('--start-date', type=str, help='Start date (YYYY-MM-DD)')
    parser.add_argument('--end-date', type=str, help='End date (YYYY-MM-DD)')
    parser.add_argument('--file-types', nargs='+', default=['broadcast'],
                       help='File types to download (default: broadcast)')
    parser.add_argument('--reprocess', action='store_true',
                       help='Reprocess failed files')
    parser.add_argument('--stats', action='store_true',
                       help='Show processing statistics')
    parser.add_argument('--config', type=str, help='Configuration file path')
    
    args = parser.parse_args()
    
    try:
        # Initialize client
        client = RinexClient()
        
        if args.stats:
            # Show statistics
            stats = client.get_statistics()
            print("\n=== RINEX Client Statistics ===")
            print(f"Files: {stats['files']['processed']}/{stats['files']['total']} processed")
            print(f"Failed: {stats['files']['failed']}, Pending: {stats['files']['pending']}")
            print(f"Navigation records: {stats['navigation_records']['total']}")
            print("\nBy satellite system:")
            for system, count in stats['navigation_records']['by_system'].items():
                print(f"  {system}: {count}")
                
        elif args.reprocess:
            # Reprocess failed files
            print("Reprocessing failed files...")
            results = client.reprocess_failed_files()
            print(f"Reprocessed {len(results['processed_files'])} files successfully")
            print(f"Failed to reprocess {len(results['failed_files'])} files")
            print(f"Total navigation records added: {results['navigation_records']}")
            
        else:
            # Process date range
            print(f"Processing RINEX files...")
            if args.start_date:
                print(f"Date range: {args.start_date} to {args.end_date or args.start_date}")
            else:
                print("Date range: Yesterday")
                
            results = client.process_date_range(
                start_date=args.start_date,
                end_date=args.end_date,
                file_types=args.file_types
            )
            
            print(f"\n=== Processing Results ===")
            print(f"Processed {len(results['processed_files'])} files successfully")
            print(f"Failed to process {len(results['failed_files'])} files")
            print(f"Total navigation records: {results['total_navigation_records']}")
            
    except KeyboardInterrupt:
        print("\nOperation cancelled by user")
        sys.exit(1)
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)


if __name__ == '__main__':
    main()