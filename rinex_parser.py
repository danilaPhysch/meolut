"""
RINEX file parser module
"""
import gzip
import logging
import re
from datetime import datetime, timedelta
from typing import List, Dict, Any, Optional, Generator
from pathlib import Path

logger = logging.getLogger(__name__)

class RinexParser:
    """Parser for RINEX navigation files"""
    
    # RINEX 3.x navigation message types
    NAV_MESSAGE_TYPES = {
        'G': 'GPS',
        'R': 'GLONASS', 
        'E': 'Galileo',
        'C': 'BeiDou',
        'J': 'QZSS',
        'I': 'IRNSS'
    }
    
    def __init__(self):
        """Initialize RINEX parser"""
        self.version = None
        self.file_type = None
        self.satellite_system = None
        self.header_info = {}
        
    def parse_file(self, file_path: str) -> Dict[str, Any]:
        """Parse RINEX file and return navigation data"""
        logger.info(f"Parsing RINEX file: {file_path}")
        
        try:
            # Determine if file is compressed
            if file_path.endswith('.gz'):
                with gzip.open(file_path, 'rt', encoding='utf-8') as f:
                    content = f.read()
            else:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    
            return self._parse_content(content)
            
        except Exception as e:
            logger.error(f"Error parsing RINEX file {file_path}: {e}")
            raise
            
    def _parse_content(self, content: str) -> Dict[str, Any]:
        """Parse RINEX file content"""
        lines = content.split('\n')
        result = {
            'header': {},
            'navigation_data': []
        }
        
        # Parse header
        header_end_idx = self._parse_header(lines, result['header'])
        
        # Parse navigation data
        if header_end_idx < len(lines):
            result['navigation_data'] = self._parse_navigation_data(lines[header_end_idx:])
            
        logger.info(f"Parsed {len(result['navigation_data'])} navigation records")
        return result
        
    def _parse_header(self, lines: List[str], header: Dict[str, Any]) -> int:
        """Parse RINEX header section"""
        for i, line in enumerate(lines):
            if 'END OF HEADER' in line:
                return i + 1
                
            # Parse version and file type
            if 'RINEX VERSION / TYPE' in line:
                parts = line[:60].split()
                if parts:
                    header['version'] = float(parts[0])
                    self.version = header['version']
                if len(parts) > 1:
                    header['file_type'] = parts[1]
                    self.file_type = parts[1]
                if len(parts) > 2:
                    header['satellite_system'] = parts[2]
                    self.satellite_system = parts[2]
                    
            # Parse program info
            elif 'PGM / RUN BY / DATE' in line:
                header['program'] = line[:20].strip()
                header['agency'] = line[20:40].strip()
                header['creation_date'] = line[40:60].strip()
                
            # Parse comment lines
            elif 'COMMENT' in line:
                if 'comments' not in header:
                    header['comments'] = []
                header['comments'].append(line[:60].strip())
                
            # Parse ionospheric corrections
            elif 'IONOSPHERIC CORR' in line:
                if 'ionospheric_corr' not in header:
                    header['ionospheric_corr'] = []
                header['ionospheric_corr'].append(line[:60].strip())
                
            # Parse time system corrections
            elif 'TIME SYSTEM CORR' in line:
                if 'time_system_corr' not in header:
                    header['time_system_corr'] = []
                header['time_system_corr'].append(line[:60].strip())
                
            # Parse leap seconds
            elif 'LEAP SECONDS' in line:
                try:
                    header['leap_seconds'] = int(line[:6].strip())
                except ValueError:
                    pass
                    
        return len(lines)
        
    def _parse_navigation_data(self, lines: List[str]) -> List[Dict[str, Any]]:
        """Parse navigation data section"""
        nav_data = []
        i = 0
        
        while i < len(lines):
            line = lines[i].strip()
            if not line:
                i += 1
                continue
                
            # Parse navigation record
            try:
                record = self._parse_nav_record(lines, i)
                if record:
                    nav_data.append(record)
                    # Skip to next record (typically 8 lines for GPS/Galileo, 4 for GLONASS)
                    i += self._get_record_lines(record.get('satellite_system', 'G'))
                else:
                    i += 1
            except Exception as e:
                logger.warning(f"Error parsing navigation record at line {i}: {e}")
                i += 1
                
        return nav_data
        
    def _parse_nav_record(self, lines: List[str], start_idx: int) -> Optional[Dict[str, Any]]:
        """Parse single navigation record"""
        if start_idx >= len(lines):
            return None
            
        first_line = lines[start_idx].strip()
        if not first_line:
            return None
            
        try:
            # Determine satellite system and ID from first character
            if len(first_line) > 0:
                sat_system_char = first_line[0]
                if sat_system_char in self.NAV_MESSAGE_TYPES:
                    satellite_system = self.NAV_MESSAGE_TYPES[sat_system_char]
                    satellite_id = first_line[:3].strip()
                else:
                    # Fallback for older RINEX versions
                    satellite_system = 'GPS'
                    satellite_id = f"G{first_line[:2].strip().zfill(2)}"
            else:
                return None
                
            # Parse epoch time from first line
            epoch_time = self._parse_epoch_time(first_line)
            if not epoch_time:
                return None
                
            # Initialize record
            record = {
                'satellite_system': satellite_system,
                'satellite_id': satellite_id,
                'epoch_time': epoch_time,
                'raw_data': '\n'.join(lines[start_idx:start_idx + self._get_record_lines(satellite_system)])
            }
            
            # Parse clock parameters from first line
            clock_params = self._parse_float_values(first_line[23:])
            if len(clock_params) >= 3:
                record['clock_bias'] = clock_params[0]
                record['clock_drift'] = clock_params[1] 
                record['clock_drift_rate'] = clock_params[2]
                
            # Parse orbital parameters from subsequent lines
            record.update(self._parse_orbital_parameters(lines, start_idx, satellite_system))
            
            return record
            
        except Exception as e:
            logger.warning(f"Error parsing navigation record: {e}")
            return None
            
    def _parse_epoch_time(self, line: str) -> Optional[datetime]:
        """Parse epoch time from RINEX line"""
        try:
            # Extract time components
            time_str = line[4:23].strip()
            parts = time_str.split()
            
            if len(parts) >= 6:
                year = int(parts[0])
                month = int(parts[1])
                day = int(parts[2])
                hour = int(parts[3])
                minute = int(parts[4])
                second = float(parts[5])
                
                # Handle 2-digit years
                if year < 80:
                    year += 2000
                elif year < 100:
                    year += 1900
                    
                # Create datetime object
                dt = datetime(year, month, day, hour, minute, int(second))
                # Add fractional seconds
                if second != int(second):
                    dt += timedelta(microseconds=int((second - int(second)) * 1000000))
                    
                return dt
                
        except (ValueError, IndexError) as e:
            logger.warning(f"Error parsing epoch time from line: {line[:30]} - {e}")
            
        return None
        
    def _parse_orbital_parameters(self, lines: List[str], start_idx: int, 
                                satellite_system: str) -> Dict[str, Any]:
        """Parse orbital parameters based on satellite system"""
        params = {}
        
        if satellite_system == 'GPS' or satellite_system == 'Galileo':
            params.update(self._parse_gps_galileo_params(lines, start_idx))
        elif satellite_system == 'GLONASS':
            params.update(self._parse_glonass_params(lines, start_idx))
        elif satellite_system == 'BeiDou':
            params.update(self._parse_beidou_params(lines, start_idx))
            
        return params
        
    def _parse_gps_galileo_params(self, lines: List[str], start_idx: int) -> Dict[str, Any]:
        """Parse GPS/Galileo orbital parameters"""
        params = {}
        
        try:
            # Line 2: IODE, Crs, Delta_n, M0
            if start_idx + 1 < len(lines):
                values = self._parse_float_values(lines[start_idx + 1])
                if len(values) >= 4:
                    params['iode'] = values[0]
                    params['crs'] = values[1]
                    params['delta_n'] = values[2]
                    params['m0'] = values[3]
                    
            # Line 3: Cuc, e, Cus, sqrt(A)
            if start_idx + 2 < len(lines):
                values = self._parse_float_values(lines[start_idx + 2])
                if len(values) >= 4:
                    params['cuc'] = values[0]
                    params['eccentricity'] = values[1]
                    params['cus'] = values[2]
                    params['sqrt_a'] = values[3]
                    
            # Line 4: TOE, Cic, OMEGA0, Cis
            if start_idx + 3 < len(lines):
                values = self._parse_float_values(lines[start_idx + 3])
                if len(values) >= 4:
                    params['toe'] = values[0]
                    params['cic'] = values[1]
                    params['omega0'] = values[2]
                    params['cis'] = values[3]
                    
            # Line 5: i0, Crc, omega, OMEGA DOT
            if start_idx + 4 < len(lines):
                values = self._parse_float_values(lines[start_idx + 4])
                if len(values) >= 4:
                    params['i0'] = values[0]
                    params['crc'] = values[1]
                    params['omega'] = values[2]
                    params['omega_dot'] = values[3]
                    
            # Line 6: IDOT, Codes on L2, GPS Week, L2 P data flag
            if start_idx + 5 < len(lines):
                values = self._parse_float_values(lines[start_idx + 5])
                if len(values) >= 2:
                    params['idot'] = values[0]
                    if len(values) >= 3:
                        params['week_number'] = int(values[2]) if values[2] is not None else None
                        
            # Line 7: SV accuracy, SV health, TGD, IODC
            if start_idx + 6 < len(lines):
                values = self._parse_float_values(lines[start_idx + 6])
                if len(values) >= 2:
                    params['accuracy'] = values[0]
                    params['health'] = int(values[1]) if values[1] is not None else None
                    
        except Exception as e:
            logger.warning(f"Error parsing GPS/Galileo parameters: {e}")
            
        return params
        
    def _parse_glonass_params(self, lines: List[str], start_idx: int) -> Dict[str, Any]:
        """Parse GLONASS orbital parameters"""
        params = {}
        
        try:
            # GLONASS uses different format - simplified implementation
            # Line 2: X, X_dot, X_ddot, health
            if start_idx + 1 < len(lines):
                values = self._parse_float_values(lines[start_idx + 1])
                if len(values) >= 4:
                    params['x_pos'] = values[0]
                    params['x_vel'] = values[1]
                    params['x_acc'] = values[2]
                    params['health'] = int(values[3]) if values[3] is not None else None
                    
            # Line 3: Y, Y_dot, Y_ddot, frequency_number
            if start_idx + 2 < len(lines):
                values = self._parse_float_values(lines[start_idx + 2])
                if len(values) >= 4:
                    params['y_pos'] = values[0]
                    params['y_vel'] = values[1]
                    params['y_acc'] = values[2]
                    params['freq_num'] = int(values[3]) if values[3] is not None else None
                    
            # Line 4: Z, Z_dot, Z_ddot, age_of_info
            if start_idx + 3 < len(lines):
                values = self._parse_float_values(lines[start_idx + 3])
                if len(values) >= 4:
                    params['z_pos'] = values[0]
                    params['z_vel'] = values[1]
                    params['z_acc'] = values[2]
                    params['age_of_info'] = values[3]
                    
        except Exception as e:
            logger.warning(f"Error parsing GLONASS parameters: {e}")
            
        return params
        
    def _parse_beidou_params(self, lines: List[str], start_idx: int) -> Dict[str, Any]:
        """Parse BeiDou orbital parameters (similar to GPS)"""
        # BeiDou uses similar format to GPS
        return self._parse_gps_galileo_params(lines, start_idx)
        
    def _parse_float_values(self, line: str) -> List[Optional[float]]:
        """Parse float values from RINEX line with scientific notation support"""
        values = []
        
        # RINEX uses fixed-width fields, typically 19 characters each
        field_width = 19
        line_padded = line.ljust(80)  # Pad to ensure we have enough characters
        
        for i in range(0, min(len(line_padded), 76), field_width):
            field = line_padded[i:i + field_width].strip()
            if field:
                try:
                    # Handle scientific notation format (replace 'D' with 'E')
                    field = field.replace('D', 'E').replace('d', 'e')
                    values.append(float(field))
                except ValueError:
                    values.append(None)
            else:
                values.append(None)
                
        return values
        
    def _get_record_lines(self, satellite_system: str) -> int:
        """Get number of lines per navigation record for different satellite systems"""
        if satellite_system == 'GLONASS':
            return 4
        else:  # GPS, Galileo, BeiDou
            return 8
            
    def validate_data(self, nav_data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Validate and filter navigation data"""
        valid_data = []
        
        for record in nav_data:
            if self._is_valid_record(record):
                valid_data.append(record)
            else:
                logger.warning(f"Invalid navigation record for {record.get('satellite_id', 'unknown')}")
                
        logger.info(f"Validated {len(valid_data)} out of {len(nav_data)} records")
        return valid_data
        
    def _is_valid_record(self, record: Dict[str, Any]) -> bool:
        """Check if navigation record is valid"""
        required_fields = ['satellite_system', 'satellite_id', 'epoch_time']
        
        for field in required_fields:
            if field not in record or record[field] is None:
                return False
                
        # Check if epoch time is reasonable (not too far in past/future)
        now = datetime.utcnow()
        epoch = record['epoch_time']
        
        if epoch < datetime(1980, 1, 1) or epoch > now + timedelta(days=30):
            return False
            
        return True