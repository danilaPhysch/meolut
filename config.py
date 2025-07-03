"""
Configuration module for RINEX client
"""
import os
from datetime import datetime, timedelta
from typing import List, Dict, Any

class Config:
    """Configuration class for RINEX client settings"""
    
    # CDDIS server configuration
    BASE_URL = "https://cddis.nasa.gov/archive/gnss/data/daily"
    
    # Supported GNSS systems
    GNSS_SYSTEMS = {
        'GPS': 'G',
        'GLONASS': 'R', 
        'Galileo': 'E',
        'BeiDou': 'C'
    }
    
    # File patterns for different RINEX types
    RINEX_PATTERNS = {
        'broadcast': 'BRDM00DLR_S_{year}{doy:03d}0000_01D_MN.rnx.gz',
        'observation': 'BRDC00IGS_R_{year}{doy:03d}0000_01D_MN.rnx.gz'
    }
    
    # Database configuration
    DATABASE_URL = os.getenv('DATABASE_URL', 'sqlite:///rinex_data.db')
    
    # Download configuration
    DOWNLOAD_DIR = os.getenv('DOWNLOAD_DIR', './downloads')
    MAX_RETRIES = int(os.getenv('MAX_RETRIES', '3'))
    TIMEOUT = int(os.getenv('TIMEOUT', '30'))
    
    # Logging configuration
    LOG_LEVEL = os.getenv('LOG_LEVEL', 'INFO')
    LOG_FILE = os.getenv('LOG_FILE', 'rinex_client.log')
    
    @classmethod
    def get_download_url(cls, date: datetime, file_type: str = 'broadcast') -> str:
        """Generate download URL for given date and file type"""
        year = date.year
        doy = date.timetuple().tm_yday  # Day of year
        
        if file_type not in cls.RINEX_PATTERNS:
            raise ValueError(f"Unsupported file type: {file_type}")
            
        filename = cls.RINEX_PATTERNS[file_type].format(year=year, doy=doy)
        return f"{cls.BASE_URL}/{year}/{doy:03d}/{year % 100:02d}n/{filename}"
    
    @classmethod
    def get_date_range(cls, start_date: str = None, end_date: str = None) -> List[datetime]:
        """Get list of dates for processing"""
        if start_date is None:
            start_date = (datetime.now() - timedelta(days=1)).strftime('%Y-%m-%d')
        if end_date is None:
            end_date = start_date
            
        start = datetime.strptime(start_date, '%Y-%m-%d')
        end = datetime.strptime(end_date, '%Y-%m-%d')
        
        dates = []
        current = start
        while current <= end:
            dates.append(current)
            current += timedelta(days=1)
            
        return dates