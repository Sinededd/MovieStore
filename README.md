# MovieStore

Веб-приложение на ASP.NET Core для просмотра каталога фильмов и управления им.

## Возможности

- Просмотр фильмов, фильтрация по жанрам и переход между страницами каталога.
- Вход и регистрация через Keycloak.
- Добавление, редактирование и удаление фильмов в панели администратора.
- Загрузка постеров фильмов.

Корзина пока демонстрационная: при добавлении фильма выводится сообщение. Оформление заказа и оплата не реализованы.

## Структура проекта

| Проект | Назначение |
| --- | --- |
| `MovieStore.UI` | Сайт: MVC, Razor Pages и обращение к API |
| `MovieStore.API` | API для работы с фильмами, жанрами и базой данных |
| `MovieStore.Domain` | Общие модели и сущности |

Используются .NET 10, Entity Framework Core, SQLite, MediatR, Keycloak, Bootstrap и jQuery. PostgreSQL в Docker нужен для хранения данных Keycloak.

## Что нужно установить

- .NET SDK 10.0.100 или более новый патч версии 10.0.1xx (см. `global.json`).
- LibMan для восстановления библиотек интерфейса.
- Docker с Docker Compose, если нужен вход и панель администратора.

Установка LibMan:

```bash
dotnet tool install --global Microsoft.Web.LibraryManager.Cli
```

## Запуск проекта

Все команды выполняются из корневой папки проекта — там, где находится `MovieStore.sln`.

### 1. Восстановить зависимости и собрать решение

```bash
libman restore
dotnet restore MovieStore.sln
dotnet build MovieStore.sln
```

Папка `wwwroot/lib` не хранится в Git. Команда `libman restore` скачивает Bootstrap, jQuery и остальные библиотеки по настройкам `libman.json`.

### 2. Запустить Keycloak

```bash
docker compose up -d
```

Дождись открытия [http://localhost:8080](http://localhost:8080). При первом запуске автоматически импортируются настройки из `keycloak/Grishko-realm.json`: realm `Grishko`, клиент `GrishkoUiClient`, роли и настройки токенов. Создавать их вручную не нужно.

Пользователи в экспорт не включены. Для регистрации через сайт и доступа к администрированию выполни шаги из раздела «Учётные записи» ниже. Если нужен только просмотр каталога, Keycloak можно не запускать.

### 3. Запустить API

В первом терминале:

```bash
dotnet run --project MovieStore.API
```

При первом запуске API автоматически создаст базу SQLite и заполнит её примерами фильмов и жанров. Вручную запускать миграции не нужно.

### 4. Запустить сайт

Во втором терминале:

```bash
dotnet run --project MovieStore.UI
```

Открой каталог: [https://localhost:7001/Movie](https://localhost:7001/Movie).

Если браузер не доверяет локальному HTTPS-сертификату, попробуй:

```bash
dotnet dev-certs https --trust
```

На Linux может потребоваться дополнительная настройка доверия сертификату в браузере.

## Учётные записи

Консоль Keycloak: [http://localhost:8080](http://localhost:8080).
Вход в консоль: логин `admin`, пароль `admin_dev_password` — они заданы в `compose.yaml` для локальной разработки. Эта учётная запись управляет Keycloak; для входа на сайт нужен отдельный пользователь realm `Grishko`.

### Разрешить регистрацию через сайт

Экспорт не содержит service account и назначенных ему прав. После первого запуска:

1. В консоли Keycloak выбери realm `Grishko`.
2. Открой **Clients → GrishkoUiClient → Service account roles**.
3. Нажми **Assign role**, найди клиент `realm-management` и назначь роль `manage-users` (при необходимости переключи фильтр на роли клиентов).

Теперь форма регистрации сайта сможет создавать пользователей.

### Создать администратора сайта

1. В realm `Grishko` открой **Users → Add user** и создай пользователя. Укажи Username и Email.
2. На вкладке **Credentials** задай пароль. Отключи **Temporary**, если смена пароля при первом входе не нужна.
3. На вкладке **Role mapping** назначь клиентскую роль **GrishkoUiClient → POWER-USER**.
4. Войди этим пользователем на сайт. Если он уже был авторизован, выйди и войди снова, чтобы обновить роли.

Панель управления фильмами: [https://localhost:7001/Admin/Movies](https://localhost:7001/Admin/Movies).

## Как хранятся настройки Keycloak

Файл `keycloak/Grishko-realm.json` хранится в Git. Docker Compose подключает его к контейнеру и запускает Keycloak с `--import-realm`.

Импорт выполняется, только если realm ещё не существует. Изменение JSON не обновляет уже созданный realm автоматически. Текущие настройки и пользователи сохраняются в Docker-томе PostgreSQL и остаются после `docker compose down`.

В экспорт включён секрет клиента для локальной разработки, совпадающий с настройкой `Keycloak:ClientSecret` сайта. Не используй эти учебные данные для публично доступного сервера.

## Адреса и настройки

| Сервис | Адрес |
| --- | --- |
| Сайт | https://localhost:7001 |
| API | http://localhost:5002/api |
| Keycloak | http://localhost:8080 |

Основные файлы настроек:

- `MovieStore.UI/appsettings.json` — подключение к API и Keycloak.
- `MovieStore.API/appsettings.json` — база SQLite и авторизация API.
- `appsettings.Development.json` в каждом проекте — настройки для локального запуска, которые переопределяют основные.
- `compose.yaml` — Keycloak и PostgreSQL.
- `keycloak/Grishko-realm.json` — начальные настройки авторизации.

База фильмов находится в `MovieStore.API/cinema.db` и не отправляется в Git.

Для остановки API и сайта нажми `Ctrl+C` в соответствующих терминалах. Для остановки Keycloak:

```bash
docker compose down
```
