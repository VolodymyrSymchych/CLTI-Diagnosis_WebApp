# Структура проєкту CLTI Diagnosis WebApp

## 📋 Зміст
1. [Загальна структура](#загальна-структура)
2. [Backend структура](#backend-структура)
3. [Frontend структура](#frontend-структура)
4. [Архітектурні рішення](#архітектурні-рішення)
5. [База даних](#база-даних)

---

## Загальна структура

```
CLTI-Diagnosis_WebApp/
├── CLTI.Diagnosis/              # Backend (Blazor Server)
├── CLTI.Diagnosis.Client/       # Frontend (Blazor WebAssembly)
├── CLTI.Diagnosis.sln          # Solution file
├── global.json                  # .NET version configuration
├── documentations/              # Документація проєкту
└── LICENSE.txt                  # Ліцензія
```

---

## Backend структура

### CLTI.Diagnosis/

```
CLTI.Diagnosis/
├── Components/                  # Blazor Server компоненти
│   ├── Account/                 # Компоненти аутентифікації
│   │   └── Pages/               # Сторінки (Login, Register, etc.)
│   ├── Layout/                  # Макети сторінок
│   │   ├── MainLayout.razor
│   │   └── MainLayout.razor.css
│   ├── Pages/                   # Додаткові сторінки
│   ├── App.razor                # Root компонент
│   ├── Routes.razor             # Маршрутизація
│   └── NotFoundComponent.razor  # 404 сторінка
│
├── Core/                        # Ядро проєкту (Domain-Driven Design)
│   ├── Application/             # Бізнес-логіка (порожня структура - TODO)
│   │   ├── Common/
│   │   │   ├── Behaviors/       # MediatR behaviors
│   │   │   ├── Interfaces/      # Інтерфейси
│   │   │   └── Models/          # Загальні моделі
│   │   ├── DTOs/                # Data Transfer Objects
│   │   ├── Features/            # Features (CQRS)
│   │   │   ├── Commands/        # Command handlers
│   │   │   └── Queries/         # Query handlers
│   │   ├── Mappings/            # AutoMapper profiles
│   │   ├── Services/            # Application services
│   │   └── Validators/          # FluentValidation validators
│   │
│   └── Domain/                  # Доменні сутності
│       ├── Entities/            # Domain entities
│       │   ├── CltiCase.cs      # Справа пацієнта
│       │   ├── CltiPhoto.cs     # Фото пацієнта
│       │   ├── SysUser.cs       # Користувач
│       │   ├── SysRole.cs       # Роль
│       │   └── ...              # Інші сутності
│       └── ValueObjects/        # Value objects (порожня - TODO)
│
├── Data/                        # Data Access Layer
│   └── ApplicationDbContext.cs  # EF Core DbContext
│
├── Infrastructure/              # Інфраструктурний шар
│   ├── Data/                    # Data infrastructure
│   │   └── Repositories/        # Repository implementations (порожня - TODO)
│   │
│   └── Services/                # Infrastructure services
│       ├── PasswordHasherService.cs      # Хешування паролів
│       ├── SessionStorageService.cs      # Зберігання сесій
│       └── SessionCacheInitializer.cs    # Ініціалізація кешу сесій
│
├── Models/                      # Моделі (DTOs)
│   └── DTOs/                    # Data Transfer Objects
│
├── Web/                         # Web Layer
│   ├── Controllers/             # API Controllers
│   │   ├── AuthController.cs    # Аутентифікація
│   │   ├── UserController.cs    # Управління користувачами
│   │   ├── CltiCaseController.cs # Управління справами
│   │   └── AiChatController.cs  # AI чат
│   │
│   ├── Middleware/              # Custom middleware
│   │   ├── SessionTokenMiddleware.cs     # Додавання токенів до запитів
│   │   └── ...                  # Інші middleware
│   │
│   └── Models/                  # Web models
│       └── Request/             # Request models
│
├── Migrations/                  # EF Core міграції
│   ├── 20251013000907_AddRefreshTokensAndPasswordHashType.cs
│   └── ...
│
├── Scripts/                     # SQL скрипти
│   └── CreateSessionCacheTable.sql
│
├── Keys/                        # Data Protection keys
│
├── logs/                        # Логи (Serilog)
│
├── wwwroot/                     # Статичні файли
│   ├── app.css
│   └── Photo/                   # Фото
│
├── Program.cs                   # Точка входу
├── appsettings.json             # Конфігурація
├── appsettings.Development.json
└── appsettings.Production.json
```

### Ключові компоненти Backend

#### Program.cs
- Налаштування сервісів
- Конфігурація аутентифікації (JWT + Cookies)
- Налаштування сесій (SQL Server Cache)
- Middleware pipeline
- CORS налаштування

#### ApplicationDbContext
- DbContext для Entity Framework Core
- Конфігурація сутностей
- Міграції

#### Controllers
- RESTful API endpoints
- Аутентифікація та авторизація
- Валідація запитів

---

## Frontend структура

### CLTI.Diagnosis.Client/

```
CLTI.Diagnosis.Client/
├── Account/                     # Аутентифікація
│   └── Pages/                   # Сторінки аутентифікації
│       ├── Login.razor
│       ├── Register.razor
│       ├── Logout.razor
│       └── ...
│
├── Algoritm/                    # Алгоритми діагностики (legacy?)
│
├── Components/                  # Переіспользувані UI компоненти
│   ├── AI_Message.razor         # Компонент AI повідомлення
│   ├── Button.razor             # Кастомна кнопка
│   ├── CheckBox.razor           # Кастомний checkbox
│   ├── DropDown.razor           # Dropdown
│   ├── Message.razor            # Повідомлення
│   ├── RadioButton.razor        # Radio button
│   ├── TextInput.razor          # Text input
│   └── ...
│
├── Features/                    # Features (Feature-based structure)
│   └── Diagnosis/               # Діагностика
│       ├── Components/          # Компоненти діагностики
│       │
│       ├── Models/              # Моделі даних
│       │   ├── Class.cs         # Загальні класи
│       │   ├── CRABData.cs      # Дані CRAB
│       │   ├── GLASSData.cs     # Дані GLASS
│       │   ├── InfectionData.cs # Дані інфекції
│       │   ├── UIState.cs       # Стан UI
│       │   ├── VascularData.cs  # Судинні дані
│       │   ├── WoundData.cs     # Дані ран
│       │   └── YLEData.cs       # Дані 2YLE
│       │
│       ├── Pages/               # Сторінки діагностики
│       │   ├── Home.razor       # Головна сторінка
│       │   ├── KPI_PPI.razor    # КПІ/ППІ
│       │   ├── WiFI_W.razor     # WiFI - Wound
│       │   ├── WiFI_I.razor     # WiFI - Ischemia
│       │   ├── WiFI_fI.razor    # WiFI - foot Infection
│       │   ├── WiFI_results.razor # Результати WiFI
│       │   ├── CRAB.razor       # CRAB оцінка
│       │   ├── _2YLE.razor      # 2YLE оцінка
│       │   ├── GLASS_*.razor    # GLASS класифікація
│       │   ├── Revascularization*.razor # Реваскуляризація
│       │   ├── AI_AssistantPage.razor # AI асистент
│       │   └── ...
│       │
│       ├── Services/            # Сервіси та калькулятори
│       │   ├── ClinicalStageCalculator.cs  # Калькулятор стадії
│       │   ├── CltiCaseService.cs          # Сервіс справ
│       │   ├── CRABCalculator.cs           # CRAB калькулятор
│       │   ├── FILevelCalculator.cs        # Інфекція калькулятор
│       │   ├── GLASSCalculator.cs          # GLASS калькулятор
│       │   ├── ILevelCalculator.cs         # Ішемія калькулятор
│       │   ├── WLevelCalculator.cs         # Рана калькулятор
│       │   └── YLECalculator.cs            # 2YLE калькулятор
│       │
│       ├── Store/               # State management (порожня - TODO)
│       │
│       └── Validation/          # Валідація (порожня - TODO)
│
├── Infrastructure/              # Інфраструктура
│   ├── Auth/                    # Аутентифікація
│   │   └── JwtAuthenticationStateProvider.cs
│   │
│   ├── Http/                    # HTTP клієнти
│   │   ├── AiChatClient.cs      # AI чат клієнт
│   │   ├── AuthApiService.cs    # Auth API
│   │   ├── CltiApiClient.cs     # CLTI API клієнт
│   │   └── ...
│   │
│   └── State/                   # State management
│       └── StateService.cs      # Глобальний стан
│
├── App/                         # App configuration
│   ├── Configuration/           # Конфігурація
│   └── Router/                  # Роутер
│
├── Core/                        # Core функціональність
│
├── Shared/                      # Спільні компоненти
│   ├── NavMenuHome.razor        # Навігаційне меню
│   └── ...
│
├── Resources/                   # Ресурси
│   ├── Localization/            # Локалізація
│   └── Static/                  # Статичні ресурси
│
├── Styles/                      # Стилі
│
├── wwwroot/                     # Статичні файли
│
└── Program.cs                   # Точка входу
```

### Ключові компоненти Frontend

#### StateService
- Глобальний стан додатку
- Зберігання даних діагностики
- Калькулятори для розрахунків
- Event-driven оновлення UI

#### Калькулятори
- Незалежні сервіси для розрахунків
- Використовують моделі даних
- Повертають результати для відображення

#### HTTP Clients
- Абстракція над HTTP запитами
- Обробка помилок
- Аутентифікація

---

## Архітектурні рішення

### Clean Architecture (частково реалізовано)

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│  (Blazor Components, Controllers)   │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│        Application Layer            │
│  (Use Cases, DTOs, Validators)      │
│  ⚠️ Порожня структура - TODO        │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│          Domain Layer               │
│  (Entities, Value Objects)          │
│  ✅ Реалізовано                     │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│      Infrastructure Layer           │
│  (Data Access, External Services)   │
│  ⚠️ Частково реалізовано            │
└─────────────────────────────────────┘
```

### Поточний стан архітектури

**Реалізовано**:
- ✅ Domain entities
- ✅ DbContext та міграції
- ✅ API Controllers
- ✅ Infrastructure services (PasswordHasher, SessionStorage)

**Потрібно реалізувати**:
- ⚠️ Application layer (CQRS, Commands, Queries)
- ⚠️ Repository pattern
- ⚠️ Unit of Work
- ⚠️ FluentValidation
- ⚠️ AutoMapper

### Аутентифікація

**Гібридна схема**:
- **Cookies** для Blazor Server (основна схема)
- **JWT** для API endpoints (додаткова схема)

**Middleware**:
- `SessionTokenMiddleware` - автоматично додає JWT токени до API запитів

### State Management

**Поточний підхід**:
- Singleton `StateService` для глобального стану
- Event-driven оновлення через `OnChange` event

**Можливі покращення**:
- Fluxor або Blazor-State для більш структурованого state management

---

## База даних

### Основні таблиці

#### Системні таблиці
- `sys_user` - Користувачі
- `sys_role` - Ролі
- `sys_rights` - Права
- `sys_user_role` - Зв'язок користувач-роль
- `sys_role_rights` - Зв'язок роль-права
- `sys_enum` - Переліки
- `sys_enum_item` - Елементи переліків
- `sys_api_key` - API ключі
- `sys_log` - Логи
- `sys_licence` - Ліцензії
- `sys_user_licence` - Зв'язок користувач-ліцензія
- `sys_refresh_token` - Refresh токени

#### Бізнес таблиці
- `u_clti` - Справи пацієнтів (CltiCase)
- `u_clti_photos` - Фото пацієнтів (CltiPhoto)
- `SessionCache` - Кеш сесій (SQL Server Distributed Cache)

### Зв'язки

```
SysUser ──┬── SysUserRole ── SysRole
          │
          ├── SysUserLicence ── SysLicence
          │
          └── SysRefreshToken

CltiCase ──┬── CltiPhoto
           │
           └── SysEnumItem (ClinicalStage)

SysEnum ── SysEnumItem
```

### Value Objects

**CltiCase містить**:
- `WifiCriteria` (owned entity) - Критерії WiFI
- `GlassCriteria` (owned entity) - Критерії GLASS

---

## Технологічний стек

### Backend
- **.NET 9.0**
- **ASP.NET Core 9.0**
- **Blazor Server**
- **Entity Framework Core 9.0**
- **SQL Server**
- **Serilog** (логуювання)
- **JWT Bearer Authentication**
- **Cookie Authentication**

### Frontend
- **.NET 9.0**
- **Blazor WebAssembly 9.0**
- **ASP.NET Core Components**

### Інфраструктура
- **SQL Server Distributed Cache** (сесії)
- **Data Protection API** (шифрування)
- **OpenAI API** (AI асистент)

---

## Порожні структури (TODO)

### Backend
- `Core/Application/Common/Behaviors/`
- `Core/Application/Common/Interfaces/`
- `Core/Application/Common/Models/`
- `Core/Application/DTOs/`
- `Core/Application/Features/Commands/`
- `Core/Application/Features/Queries/`
- `Core/Application/Mappings/`
- `Core/Application/Services/`
- `Core/Application/Validators/`
- `Core/Domain/Enums/`
- `Core/Domain/Events/`
- `Core/Domain/Exceptions/`
- `Core/Domain/ValueObjects/`
- `Infrastructure/Data/Repositories/`
- `Infrastructure/Services/Email/`
- `Infrastructure/Services/Identity/`
- `Infrastructure/Services/Storage/`

### Frontend
- `Features/Diagnosis/Store/`
- `Features/Diagnosis/Validation/`

---

## Рекомендації по розвитку

1. **Заповнити Application Layer**:
   - Реалізувати CQRS pattern
   - Додати Commands та Queries
   - Використати MediatR

2. **Реалізувати Repository Pattern**:
   - Generic repository
   - Unit of Work
   - Спеціалізовані репозиторії

3. **Додати валідацію**:
   - FluentValidation для Commands
   - Валідація на фронтенді

4. **Покращити State Management**:
   - Використати Fluxor або Blazor-State
   - Розділити state по features

5. **Додати тести**:
   - Unit тести для калькуляторів
   - Integration тести для API
   - E2E тести для критичних сценаріїв

---

**Останнє оновлення**: 2025-01-XX  
**Версія документа**: 1.0

