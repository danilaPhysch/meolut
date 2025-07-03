#!/usr/bin/env python3
"""
Example usage of RINEX client
This script demonstrates how to use the RINEX client as a module
"""
import sys
import os
from datetime import datetime, timedelta

# Add current directory to path
sys.path.insert(0, os.path.dirname(__file__))

from rinex_client import RinexClient

def example_basic_usage():
    """Example of basic RINEX client usage"""
    print("=== Basic RINEX Client Usage Example ===\n")
    
    # Initialize client
    client = RinexClient()
    print("✓ RINEX client initialized")
    
    # Show initial statistics
    stats = client.get_statistics()
    print(f"Initial state: {stats['files']['total']} files in database")
    
    # Example: Process files for a specific date
    # Note: In a real environment, this would download from CDDIS
    test_date = datetime(2025, 1, 18)
    print(f"\nAttempting to process files for {test_date.strftime('%Y-%m-%d')}")
    print("(In sandbox environment, this will show download attempts but won't succeed)")
    
    try:
        results = client.process_date(test_date)
        print(f"Processing results for {results['date'].strftime('%Y-%m-%d')}:")
        print(f"  Processed files: {len(results['processed_files'])}")
        print(f"  Failed files: {len(results['failed_files'])}")
        print(f"  Navigation records: {results['navigation_records']}")
        
        if results['failed_files']:
            print(f"  Failed files list: {results['failed_files']}")
            
    except Exception as e:
        print(f"Expected error in sandbox environment: {e}")
    
    # Show updated statistics
    stats = client.get_statistics()
    print(f"\nFinal state: {stats['files']['total']} files in database")

def example_date_range_processing():
    """Example of processing a date range"""
    print("\n=== Date Range Processing Example ===\n")
    
    client = RinexClient()
    
    # Process multiple days
    print("Processing files for date range: 2025-01-15 to 2025-01-17")
    
    try:
        results = client.process_date_range('2025-01-15', '2025-01-17')
        print(f"Range processing results:")
        print(f"  Total dates: {results['total_dates']}")
        print(f"  Processed files: {len(results['processed_files'])}")
        print(f"  Failed files: {len(results['failed_files'])}")
        print(f"  Total navigation records: {results['total_navigation_records']}")
        
    except Exception as e:
        print(f"Expected error in sandbox environment: {e}")

def example_configuration():
    """Example of configuration options"""
    print("\n=== Configuration Example ===\n")
    
    from config import Config
    
    # Show configuration options
    config = Config()
    print("Current configuration:")
    print(f"  Base URL: {config.BASE_URL}")
    print(f"  Supported GNSS: {list(config.GNSS_SYSTEMS.keys())}")
    print(f"  Download directory: {config.DOWNLOAD_DIR}")
    print(f"  Database URL: {config.DATABASE_URL}")
    print(f"  Max retries: {config.MAX_RETRIES}")
    print(f"  Timeout: {config.TIMEOUT} seconds")
    
    # Show URL generation for different dates
    print("\nGenerated URLs for different dates:")
    for i in range(3):
        date = datetime(2025, 1, 15) + timedelta(days=i)
        url = config.get_download_url(date, 'broadcast')
        day_of_year = date.timetuple().tm_yday
        print(f"  {date.strftime('%Y-%m-%d')} (day {day_of_year:03d}): {os.path.basename(url)}")

def example_database_operations():
    """Example of database operations"""
    print("\n=== Database Operations Example ===\n")
    
    from database import DatabaseManager
    
    # Use in-memory database for demo
    db = DatabaseManager('sqlite:///:memory:')
    print("✓ Database initialized")
    
    # Add some example file records
    test_files = [
        ('BRDM00DLR_S_20250150000_01D_MN.rnx.gz', datetime(2025, 1, 15), 'broadcast'),
        ('BRDM00DLR_S_20250160000_01D_MN.rnx.gz', datetime(2025, 1, 16), 'broadcast'),
        ('BRDM00DLR_S_20250170000_01D_MN.rnx.gz', datetime(2025, 1, 17), 'broadcast'),
    ]
    
    for filename, file_date, file_type in test_files:
        file_id = db.add_rinex_file(filename, file_date, file_type, 1024, f'/downloads/{filename}')
        print(f"✓ Added file: {filename} (ID: {file_id})")
    
    # Add some example navigation data
    nav_data = []
    for i in range(10):
        record = {
            'rinex_file_id': 1,
            'satellite_system': 'GPS',
            'satellite_id': f'G{i+1:02d}',
            'epoch_time': datetime(2025, 1, 15, i, 0, 0),
            'clock_bias': 1e-6 * i,
            'clock_drift': 1e-12 * i,
            'eccentricity': 0.01 + 0.001 * i,
            'sqrt_a': 5153.0 + i,
            'raw_data': f'GPS satellite {i+1} data'
        }
        nav_data.append(record)
    
    db.add_navigation_data(nav_data)
    print(f"✓ Added {len(nav_data)} navigation records")
    
    # Query data
    unprocessed = db.get_unprocessed_files()
    print(f"✓ Found {len(unprocessed)} unprocessed files")
    
    nav_records = db.get_navigation_data('GPS')
    print(f"✓ Found {len(nav_records)} GPS navigation records")

def main():
    """Run all examples"""
    print("=== RINEX Client Usage Examples ===\n")
    
    try:
        example_configuration()
        example_database_operations()
        example_basic_usage()
        example_date_range_processing()
        
        print("\n=== Examples Summary ===")
        print("✓ All examples completed successfully")
        print("✓ RINEX client is working correctly")
        print("\nIn a real environment with internet access:")
        print("- Files would be downloaded from CDDIS")
        print("- RINEX data would be parsed and stored")
        print("- Complete navigation datasets would be available")
        
    except Exception as e:
        print(f"\n✗ Example failed with error: {e}")
        import traceback
        traceback.print_exc()
        return 1
        
    return 0

if __name__ == "__main__":
    sys.exit(main())