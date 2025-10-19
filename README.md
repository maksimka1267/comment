Готово! Нижче — готовий README українською для вставки в репозиторій. Я врахував, що це **MVC-приклад** на **ASP.NET Core 8**, фронт — **звичайна HTML+CSS+JS** сторінка, БД — **SQL Server**, порт **7099**, і твій `appsettings.json`.

---

# 🧩 SPA «Коментарі» — ASP.NET Core 8 MVC + EF Core + SQL Server

Мінізастосунок (приклад **MVC**) для публікації та перегляду коментарів із вкладеними відповідями, пагінацією, сортуванням і завантаженням файлів. Фронтенд реалізований як звичайна **HTML + CSS + JS** сторінка без окремого фреймворка.

## Зміст

* [Технології](#технології)
* [Можливості](#можливості)
* [Структура проєкту (діаграма)](#структура-проєкту-діаграма)
* [Швидкий старт](#швидкий-старт)
* [Міграції бази даних](#міграції-бази-даних)
* [Пагінація, сортування та вкладені відповіді](#пагінація-сортування-та-вкладені-відповіді)
* [API (огляд)](#api-огляд)
* [Роадмап](#роадмап)

---

## Технології

* **.NET 8 / ASP.NET Core MVC**
* **Entity Framework Core** (Code-First, Migrations)
* **SQL Server**
* **WebSockets** для оновлень у реальному часі (повідомлення про нові коментарі)
* Чиста **HTML/CSS/JS** сторінка як фронтенд (без React/Vue/Angular)

---

## Можливості

* Форма додавання коментаря: *User Name*, *Email*, *Home page*, *CAPTCHA*, *Text* (дозволені тільки `<a>`, `<code>`, `<i>`, `<strong>`).
* Вкладені/каскадні відповіді на коментарі.
* Сортування заголовних коментарів за **User Name / Email / датою** у двох напрямах; за замовчуванням — **LIFO**.
* Пагінація: **25 записів на сторінку**.
* Завантаження файлів: зображення **JPG/GIF/PNG** (автомасштаб до **320×240**) і **.txt** до **100 KB**; перегляд із lightbox-ефектом.
* Захист від **XSS** (фільтрація HTML) і **SQL-ін’єкцій** (EF Core / параметризовані запити).

---

## Структура проєкту (діаграма)

```text
comment/
├─ Connected Services/
├─ Properties/
├─ wwwroot/                     # статика (css, js, зображення, lightbox)
├─ Controllers/
│  ├─ CommentController.cs      # CRUD/Replies/Sorting/Pagination
│  └─ HomeController.cs         # Index (SPA shell)
├─ Data/
│  ├─ Model/
│  │  ├─ Comment.cs             # сутність коментаря (ParentId для вкладеності)
│  │  ├─ Attachment.cs          # сутність вкладень
│  │  ├─ AttachmentType.cs
│  │  ├─ CommentQueueItem.cs    # елемент черги обробки
│  │  └─ RECaptcha.cs           # DTO/модель для CAPTCHA
│  └─ AppDbContext.cs           # EF Core DbContext
├─ EventHandlers/
│  └─ CommentAddedEventHandler.cs
├─ Events/
│  └─ CommentAddedEvent.cs
├─ Interface/
│  ├─ IAttachmentRepository.cs
│  └─ ICommentRepository.cs
├─ Migrations/                  # EF Core міграції
├─ Repository/
│  ├─ AttachmentRepository.cs
│  └─ CommentRepository.cs
├─ Services/
│  ├─ Interface/
│  │  └─ IQueueService.cs
│  ├─ Repository/
│  │  └─ QueueService.cs
│  └─ WebSocket/
│     └─ WebSocketHandler.cs
├─ Views/
│  └─ Home/
│     └─ Index.cshtml           # HTML + CSS + JS UI
├─ appsettings.json
├─ appsettings.Development.json # (необов’язково; локальні налаштування)
└─ Program.cs
```

---

## Швидкий старт

### Вимоги

* **.NET SDK 8.0**
* **SQL Server** (локально або у контейнері Docker)
* Windows/Linux/macOS — без різниці

### Клонування та запуск

```bash
git clone https://github.com/<your-account>/<your-repo>.git
cd <your-repo>

# 1) ОНОВИ рядок підключення у appsettings.json або у appsettings.Development.json
# 2) Застосуй міграції БД
dotnet tool install --global dotnet-ef
dotnet ef database update

# 3) Запусти застосунок
dotnet run
```

Після запуску застосунок слухає **Kestrel** на `http://localhost:7099`. Головна сторінка — `GET /` (віддає `Views/Home/Index.cshtml`).

---

## Міграції бази даних

```bash
# створити міграцію
dotnet ef migrations add Init

# застосувати міграції
dotnet ef database update

# відкотити до попереднього стану
dotnet ef database update <PreviousMigrationName>
```

---

## Пагінація, сортування та вкладені відповіді

* Пагінація: **25 записів/сторінка** (query-параметри `page`, `pageSize`).
* Сортування: `sort=UserName|Email|Date` і `dir=asc|desc`; за замовчуванням — **LIFO**.
* Вкладеність: у сутності `Comment` використовується `ParentId` для формування дерева відповідей.

---

## API (огляд)

> Узагальнений огляд за ролями контролерів. Якщо потрібно — додам повну таблицю ендпойнтів (URL/метод/тіло/відповідь) з прикладами `curl`.

* `GET /api/comments` — отримати список заголовних коментарів (з пагінацією/сортуванням).
* `GET /api/comments/{id}` — отримати коментар із вкладеними відповідями.
* `POST /api/comments` — створити новий коментар (валідація CAPTCHA, фільтрація HTML).
* `POST /api/comments/{id}/reply` — додати відповідь.
* `POST /api/attachments` — завантажити файл (перевірка типу/розміру).
* `GET /attachments/{id}` — завантажити/переглянути вкладення.
* WebSocket: `ws://localhost:7099/ws` — пуш оновлень для нових коментарів.

---

## Роадмап

* [ ] **Swagger/OpenAPI** для документування API.
* [ ] **Unit/Integration tests** на репозиторії і контролери.
* [ ] **Dockerfile / docker-compose** (API + SQL Server) для швидкого підняття стека.
* [ ] Seed-дані для швидкого демо.



Хочеш — надішли мені точні маршрути/DTO з `CommentController.cs`, і я розпишу повну таблицю API + приклади запитів/відповідей. Якщо додаси `Dockerfile` — включу розділ «Запуск у Docker».
