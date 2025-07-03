"""
Database module for storing RINEX data
"""
import logging
from datetime import datetime
from typing import List, Dict, Any, Optional
from sqlalchemy import create_engine, Column, Integer, String, DateTime, Float, Text, Index
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker, Session
from config import Config

logger = logging.getLogger(__name__)

Base = declarative_base()

class RinexFile(Base):
    """Table for tracking downloaded RINEX files"""
    __tablename__ = 'rinex_files'
    
    id = Column(Integer, primary_key=True)
    filename = Column(String(255), unique=True, nullable=False)
    download_date = Column(DateTime, default=datetime.utcnow)
    file_date = Column(DateTime, nullable=False)
    file_type = Column(String(50), nullable=False)  # broadcast, observation, etc.
    file_size = Column(Integer)
    file_path = Column(String(512))
    processed = Column(String(10), default='pending')  # pending, processed, error
    checksum = Column(String(64))
    
    __table_args__ = (
        Index('idx_file_date', 'file_date'),
        Index('idx_filename', 'filename'),
        Index('idx_processed', 'processed'),
    )

class NavigationData(Base):
    """Table for storing parsed navigation data from RINEX files"""
    __tablename__ = 'navigation_data'
    
    id = Column(Integer, primary_key=True)
    rinex_file_id = Column(Integer, nullable=False)
    satellite_system = Column(String(10), nullable=False)  # GPS, GLONASS, Galileo, BeiDou
    satellite_id = Column(String(10), nullable=False)
    epoch_time = Column(DateTime, nullable=False)
    clock_bias = Column(Float)
    clock_drift = Column(Float)
    clock_drift_rate = Column(Float)
    # Orbital parameters
    iode = Column(Float)  # Issue of Data Ephemeris
    crs = Column(Float)   # Sine harmonic correction to orbit radius
    delta_n = Column(Float)  # Mean motion difference
    m0 = Column(Float)    # Mean anomaly at reference time
    cuc = Column(Float)   # Cosine harmonic correction to argument of latitude
    eccentricity = Column(Float)
    cus = Column(Float)   # Sine harmonic correction to argument of latitude
    sqrt_a = Column(Float)  # Square root of semi-major axis
    toe = Column(Float)   # Time of ephemeris
    cic = Column(Float)   # Cosine harmonic correction to inclination angle
    omega0 = Column(Float)  # Longitude of ascending node
    cis = Column(Float)   # Sine harmonic correction to inclination angle
    i0 = Column(Float)    # Inclination angle at reference time
    crc = Column(Float)   # Cosine harmonic correction to orbit radius
    omega = Column(Float)  # Argument of perigee
    omega_dot = Column(Float)  # Rate of right ascension
    idot = Column(Float)  # Rate of inclination angle
    # Additional parameters
    week_number = Column(Integer)
    accuracy = Column(Float)
    health = Column(Integer)
    raw_data = Column(Text)  # Store original RINEX record for reference
    
    __table_args__ = (
        Index('idx_satellite_system', 'satellite_system'),
        Index('idx_satellite_id', 'satellite_id'),
        Index('idx_epoch_time', 'epoch_time'),
        Index('idx_rinex_file_id', 'rinex_file_id'),
    )

class DatabaseManager:
    """Database manager for RINEX data operations"""
    
    def __init__(self, database_url: str = None):
        """Initialize database manager"""
        self.database_url = database_url or Config.DATABASE_URL
        self.engine = create_engine(self.database_url, echo=False)
        self.SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=self.engine)
        self.create_tables()
        
    def create_tables(self):
        """Create database tables if they don't exist"""
        try:
            Base.metadata.create_all(bind=self.engine)
            logger.info("Database tables created successfully")
        except Exception as e:
            logger.error(f"Error creating database tables: {e}")
            raise
            
    def get_session(self) -> Session:
        """Get database session"""
        return self.SessionLocal()
        
    def add_rinex_file(self, filename: str, file_date: datetime, file_type: str, 
                      file_size: int = None, file_path: str = None, 
                      checksum: str = None) -> int:
        """Add RINEX file record to database"""
        with self.get_session() as session:
            try:
                rinex_file = RinexFile(
                    filename=filename,
                    file_date=file_date,
                    file_type=file_type,
                    file_size=file_size,
                    file_path=file_path,
                    checksum=checksum
                )
                session.add(rinex_file)
                session.commit()
                session.refresh(rinex_file)
                logger.info(f"Added RINEX file record: {filename}")
                return rinex_file.id
            except Exception as e:
                session.rollback()
                logger.error(f"Error adding RINEX file record: {e}")
                raise
                
    def update_file_status(self, file_id: int, status: str):
        """Update file processing status"""
        with self.get_session() as session:
            try:
                rinex_file = session.query(RinexFile).filter(RinexFile.id == file_id).first()
                if rinex_file:
                    rinex_file.processed = status
                    session.commit()
                    logger.info(f"Updated file {file_id} status to {status}")
                else:
                    logger.warning(f"RINEX file with ID {file_id} not found")
            except Exception as e:
                session.rollback()
                logger.error(f"Error updating file status: {e}")
                raise
                
    def file_exists(self, filename: str) -> bool:
        """Check if file already exists in database"""
        with self.get_session() as session:
            try:
                return session.query(RinexFile).filter(RinexFile.filename == filename).first() is not None
            except Exception as e:
                logger.error(f"Error checking file existence: {e}")
                return False
                
    def add_navigation_data(self, nav_data: List[Dict[str, Any]]) -> int:
        """Add navigation data records to database"""
        with self.get_session() as session:
            try:
                records = []
                for data in nav_data:
                    record = NavigationData(**data)
                    records.append(record)
                
                session.add_all(records)
                session.commit()
                logger.info(f"Added {len(records)} navigation data records")
                return len(records)
            except Exception as e:
                session.rollback()
                logger.error(f"Error adding navigation data: {e}")
                raise
                
    def get_unprocessed_files(self) -> List[RinexFile]:
        """Get list of unprocessed RINEX files"""
        with self.get_session() as session:
            try:
                return session.query(RinexFile).filter(RinexFile.processed == 'pending').all()
            except Exception as e:
                logger.error(f"Error getting unprocessed files: {e}")
                return []
                
    def get_navigation_data(self, satellite_system: str = None, 
                          start_date: datetime = None, 
                          end_date: datetime = None) -> List[NavigationData]:
        """Get navigation data with optional filters"""
        with self.get_session() as session:
            try:
                query = session.query(NavigationData)
                
                if satellite_system:
                    query = query.filter(NavigationData.satellite_system == satellite_system)
                if start_date:
                    query = query.filter(NavigationData.epoch_time >= start_date)
                if end_date:
                    query = query.filter(NavigationData.epoch_time <= end_date)
                    
                return query.all()
            except Exception as e:
                logger.error(f"Error getting navigation data: {e}")
                return []