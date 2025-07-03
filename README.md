# RINEX Client

Автоматический клиент для скачивания, парсинга и сохранения в базу данных RINEX файлов с сервера NASA CDDIS.

## Возможности

- Автоматическое скачивание актуальных RINEX файлов с https://cddis.nasa.gov/archive/gnss/data/daily/
- Поддержка всех основных GNSS систем: GPS, GLONASS, Galileo, BeiDou
- Парсинг навигационных сообщений из RINEX файлов
- Сохранение данных в базу данных SQLite/SQLAlchemy
- Обработка сжатых файлов (.gz)
- Подробное логирование процесса
- Конфигурируемые параметры
- Валидация и проверка целостности данных

## Установка

1. Клонируйте репозиторий:
```bash
git clone https://github.com/danilaPhysch/meolut.git
cd meolut
```

2. Установите зависимости:
```bash
pip install -r requirements.txt
```

## Использование

### Командная строка

Скачать и обработать файлы за вчерашний день:
```bash
python rinex_client.py
```

Скачать файлы за определенную дату:
```bash
python rinex_client.py --start-date 2025-01-18
```

Скачать файлы за диапазон дат:
```bash
python rinex_client.py --start-date 2025-01-15 --end-date 2025-01-18
```

Переобработать файлы с ошибками:
```bash
python rinex_client.py --reprocess
```

Показать статистику обработки:
```bash
python rinex_client.py --stats
```

### Использование как модуль

```python
from rinex_client import RinexClient
from datetime import datetime

# Создание клиента
client = RinexClient()

# Обработка файлов за определенную дату
date = datetime(2025, 1, 18)
results = client.process_date(date)

# Обработка диапазона дат
results = client.process_date_range('2025-01-15', '2025-01-18')

# Получение статистики
stats = client.get_statistics()
print(f"Обработано файлов: {stats['files']['processed']}")
```

## Структура проекта

```
meolut/
├── rinex_client.py     # Основной модуль клиента
├── database.py         # Модуль для работы с базой данных
├── rinex_parser.py     # Парсер RINEX файлов
├── config.py           # Конфигурация
├── requirements.txt    # Зависимости Python
├── README.md          # Документация
└── downloads/         # Директория для загруженных файлов (создается автоматически)
```

## Конфигурация

Настройки можно изменить через переменные окружения:

- `DATABASE_URL` - URL базы данных (по умолчанию: sqlite:///rinex_data.db)
- `DOWNLOAD_DIR` - Директория для загрузок (по умолчанию: ./downloads)
- `MAX_RETRIES` - Количество попыток загрузки (по умолчанию: 3)
- `TIMEOUT` - Таймаут запроса в секундах (по умолчанию: 30)
- `LOG_LEVEL` - Уровень логирования (по умолчанию: INFO)
- `LOG_FILE` - Файл логов (по умолчанию: rinex_client.log)

Пример:
```bash
export DATABASE_URL="postgresql://user:pass@localhost/rinex"
export DOWNLOAD_DIR="/data/rinex"
export LOG_LEVEL="DEBUG"
python rinex_client.py --start-date 2025-01-18
```

## Структура базы данных

### Таблица rinex_files
Информация о загруженных RINEX файлах:
- `id` - Уникальный идентификатор
- `filename` - Имя файла
- `download_date` - Дата загрузки
- `file_date` - Дата данных в файле
- `file_type` - Тип файла (broadcast, observation)
- `file_size` - Размер файла
- `file_path` - Путь к файлу
- `processed` - Статус обработки (pending, processed, error)
- `checksum` - MD5 контрольная сумма

### Таблица navigation_data
Навигационные данные из RINEX файлов:
- `id` - Уникальный идентификатор
- `rinex_file_id` - Ссылка на файл
- `satellite_system` - Спутниковая система (GPS, GLONASS, Galileo, BeiDou)
- `satellite_id` - Идентификатор спутника
- `epoch_time` - Время эпохи
- `clock_bias` - Смещение часов
- `clock_drift` - Скорость ухода часов
- Орбитальные параметры (eccentricity, sqrt_a, omega0, etc.)
- `raw_data` - Исходные данные RINEX

## Поддерживаемые форматы

- RINEX 2.x и 3.x навигационные файлы
- Файлы типа BRDM (Mixed GNSS broadcast)
- Сжатые файлы .gz
- Спутниковые системы: GPS, GLONASS, Galileo, BeiDou, QZSS, IRNSS

## Типичные файлы CDDIS

Клиент автоматически формирует URL для файлов типа:
- `BRDM00DLR_S_20251830000_01D_MN.rnx.gz` - Mixed GNSS broadcast
- `BRDC00IGS_R_20251830000_01D_MN.rnx.gz` - IGS broadcast

Где:
- `2025183` - год и день года (день 183 в 2025 году)
- `MN` - Mixed Navigation data

## Обработка ошибок

Клиент включает обработку следующих ошибок:
- Сетевые ошибки при загрузке
- Ошибки файловой системы
- Ошибки парсинга RINEX файлов
- Ошибки базы данных
- Автоматические повторные попытки
- Логирование всех операций

## Мониторинг

Используйте команду `--stats` для мониторинга:
```bash
python rinex_client.py --stats
```

Выведет:
- Количество обработанных файлов
- Статистику по спутниковым системам
- Общее количество навигационных записей
- Статус обработки файлов

## Требования

- Python 3.8+
- Интернет соединение для загрузки с CDDIS
- Не менее 100 МБ свободного места для файлов
- SQLite (входит в Python) или PostgreSQL для хранения данных

## Лицензия

MIT License - см. файл LICENSE для подробностей.

## Поддержка

При возникновении проблем:
1. Проверьте логи в файле `rinex_client.log`
2. Убедитесь, что CDDIS сервер доступен
3. Проверьте права доступа к директории загрузок
4. Используйте флаг `--reprocess` для повторной обработки ошибочных файлов