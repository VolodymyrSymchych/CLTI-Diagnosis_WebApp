# Детальний розбір задач проєкту CLTI Diagnosis

## 📋 Зміст
1. [Фаза 1: Стабілізація](#фаза-1-стабілізація)
2. [Фаза 2: Розширення функціональності](#фаза-2-розширення-функціональності)
3. [Фаза 3: Інтеграції](#фаза-3-інтеграції)
4. [Фаза 4: Підприємницькі функції](#фаза-4-підприємницькі-функції)
5. [Шаблони задач](#шаблони-задач)

---

## Фаза 1: Стабілізація

### Epic 1.1: Тестування та якість коду

#### Task 1.1.1: Unit тести для калькуляторів
**ID**: TASK-101  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 5  
**Оцінка часу**: 3-5 днів

**Опис**:
Створити unit тести для всіх калькуляторів діагностики, щоб забезпечити правильність розрахунків.

**Acceptance Criteria**:
- [ ] Тести для WLevelCalculator покривають всі сценарії
- [ ] Тести для ILevelCalculator покривають всі сценарії
- [ ] Тести для FILevelCalculator покривають всі сценарії
- [ ] Тести для CRABCalculator покривають всі сценарії
- [ ] Тести для YLECalculator покривають всі сценарії
- [ ] Тести для GLASSCalculator покривають всі сценарії
- [ ] Тести для ClinicalStageCalculator покривають всі сценарії
- [ ] Покриття коду >= 90% для всіх калькуляторів
- [ ] Всі тести проходять в CI/CD

**Технічні деталі**:
- Використати xUnit або NUnit
- Тестові дані мають покривати edge cases
- Тести мають бути незалежними та швидкими

**Залежності**: Немає

**Підзадачі**:
1. Налаштування тестового проєкту
2. Тести для WLevelCalculator
3. Тести для ILevelCalculator
4. Тести для FILevelCalculator
5. Тести для CRABCalculator
6. Тести для YLECalculator
7. Тести для GLASSCalculator
8. Тести для ClinicalStageCalculator
9. Інтеграція в CI/CD

---

#### Task 1.1.2: Unit тести для сервісів
**ID**: TASK-102  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 8  
**Оцінка часу**: 5-7 днів

**Опис**:
Створити unit тести для бізнес-сервісів з використанням моків для залежностей.

**Acceptance Criteria**:
- [ ] Тести для PasswordHasherService
- [ ] Тести для JwtTokenService
- [ ] Тести для UserService
- [ ] Тести для CltiCaseService
- [ ] Тести для SessionStorageService
- [ ] Покриття коду >= 80% для всіх сервісів
- [ ] Використання моків для залежностей (Moq/NSubstitute)

**Технічні деталі**:
- Моки для DbContext, HttpClient, ILogger
- Тести для success та error сценаріїв
- Перевірка валідації

**Залежності**: TASK-101

---

#### Task 1.1.3: Integration тести для API
**ID**: TASK-103  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 13  
**Оцінка часу**: 7-10 днів

**Опис**:
Створити integration тести для всіх API endpoints з використанням тестової бази даних.

**Acceptance Criteria**:
- [ ] Тести для AuthController
- [ ] Тести для UserController
- [ ] Тести для CltiCaseController
- [ ] Тести для AiChatController
- [ ] Тести для middleware
- [ ] Використання тестової БД (InMemory або TestContainer)
- [ ] Тести для authentication flows
- [ ] Тести для authorization

**Технічні деталі**:
- WebApplicationFactory для тестування
- Тестова БД очищається після кожного тесту
- Тести для різних ролей користувачів

**Залежності**: TASK-102

---

### Epic 1.2: Рефакторинг архітектури

#### Task 1.2.1: Реалізація CQRS pattern
**ID**: TASK-121  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 21  
**Оцінка часу**: 10-15 днів

**Опис**:
Реалізувати CQRS (Command Query Responsibility Segregation) pattern для покращення архітектури.

**Acceptance Criteria**:
- [ ] Структура Commands (Create, Update, Delete)
- [ ] Структура Queries (Get, List, Search)
- [ ] Використання MediatR
- [ ] Handlers для всіх команд та запитів
- [ ] Міграція існуючого коду на CQRS
- [ ] Документація pattern

**Технічні деталі**:
```
Core/Application/
├── Features/
│   ├── Cases/
│   │   ├── Commands/
│   │   │   ├── CreateCase/
│   │   │   ├── UpdateCase/
│   │   │   └── DeleteCase/
│   │   └── Queries/
│   │       ├── GetCase/
│   │       └── ListCases/
│   └── Users/
│       └── ...
```

**Залежності**: Немає

**Підзадачі**:
1. Встановлення MediatR
2. Створення базових класів (Command, Query, Handler)
3. Реалізація Commands для Cases
4. Реалізація Queries для Cases
5. Реалізація Commands для Users
6. Реалізація Queries для Users
7. Міграція контролерів на використання MediatR
8. Тестування

---

#### Task 1.2.2: Реалізація Repository pattern
**ID**: TASK-122  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 13  
**Оцінка часу**: 7-10 днів

**Опис**:
Реалізувати Generic Repository pattern для абстракції доступу до даних.

**Acceptance Criteria**:
- [ ] IRepository<T> інтерфейс
- [ ] Repository<T> реалізація
- [ ] Спеціалізовані репозиторії (ICaseRepository, IUserRepository)
- [ ] Unit of Work pattern
- [ ] Міграція існуючого коду
- [ ] Тести для репозиторіїв

**Технічні деталі**:
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

**Залежності**: TASK-121

---

#### Task 1.2.3: Додавання FluentValidation
**ID**: TASK-123  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 8  
**Оцінка часу**: 5-7 днів

**Опис**:
Додати FluentValidation для валідації команд та DTOs.

**Acceptance Criteria**:
- [ ] Встановлення FluentValidation
- [ ] Validators для всіх Commands
- [ ] Validators для DTOs
- [ ] Інтеграція з MediatR pipeline
- [ ] Повідомлення про помилки валідації
- [ ] Тести для валідаторів

**Технічні деталі**:
- Behavior для автоматичної валідації
- Локалізація повідомлень про помилки

**Залежності**: TASK-121

---

#### Task 1.2.4: Додавання AutoMapper
**ID**: TASK-124  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 5  
**Оцінка часу**: 3-5 днів

**Опис**:
Додати AutoMapper для маппінгу між сутностями та DTOs.

**Acceptance Criteria**:
- [ ] Встановлення AutoMapper
- [ ] Mapping profiles для всіх сутностей
- [ ] Заміна ручного маппінгу на AutoMapper
- [ ] Тести для маппінгу

**Залежності**: TASK-121

---

### Epic 1.3: Оптимізація продуктивності

#### Task 1.3.1: Аналіз та оптимізація запитів до БД
**ID**: TASK-131  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 13  
**Оцінка часу**: 7-10 днів

**Опис**:
Проаналізувати та оптимізувати запити до бази даних.

**Acceptance Criteria**:
- [ ] Аналіз повільних запитів (SQL Profiler)
- [ ] Додавання індексів
- [ ] Оптимізація LINQ запитів
- [ ] Використання AsNoTracking()
- [ ] Пагінація для всіх списків
- [ ] Документація змін

**Технічні деталі**:
- Індекси для часто використовуваних полів
- Composite indexes для складних запитів
- Query splitting для Include()

**Підзадачі**:
1. Аналіз поточних запитів
2. Визначення повільних запитів
3. Створення індексів
4. Оптимізація LINQ
5. Додавання пагінації
6. Тестування продуктивності

---

#### Task 1.3.2: Кешування
**ID**: TASK-132  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 8  
**Оцінка часу**: 5-7 днів

**Опис**:
Додати кешування для часто використовуваних даних.

**Acceptance Criteria**:
- [ ] Кешування enum значень
- [ ] Кешування користувацьких даних
- [ ] Кешування результатів діагностики
- [ ] Cache invalidation стратегія
- [ ] Налаштування TTL

**Технічні деталі**:
- IMemoryCache для in-memory кешування
- Distributed cache для production (Redis)

**Залежності**: TASK-131

---

### Epic 1.4: Документація

#### Task 1.4.1: Swagger документація
**ID**: TASK-141  
**Тип**: Documentation  
**Пріоритет**: High  
**Story Points**: 5  
**Оцінка часу**: 3-5 днів

**Опис**:
Налаштувати та задокументувати API через Swagger/OpenAPI.

**Acceptance Criteria**:
- [ ] Swagger UI налаштовано
- [ ] Всі endpoints задокументовані
- [ ] Приклади запитів/відповідей
- [ ] Authentication схеми
- [ ] Моделі даних описані

**Технічні деталі**:
- Swashbuckle.AspNetCore
- XML comments для документації
- Приклади для кожного endpoint

---

#### Task 1.4.2: Технічна документація
**ID**: TASK-142  
**Тип**: Documentation  
**Пріоритет**: Medium  
**Story Points**: 8  
**Оцінка часу**: 5-7 днів

**Опис**:
Створити повну технічну документацію проєкту.

**Acceptance Criteria**:
- [ ] Документація архітектури
- [ ] Deployment guide
- [ ] Database schema документація
- [ ] Environment setup guide
- [ ] Troubleshooting guide

---

## Фаза 2: Розширення функціональності

### Epic 2.1: Експорт звітів

#### Task 2.1.1: PDF генерація
**ID**: TASK-211  
**Тип**: Development  
**Пріоритет**: High  
**Story Points**: 13  
**Оцінка часу**: 7-10 днів

**Опис**:
Реалізувати генерацію PDF звітів для справ.

**Acceptance Criteria**:
- [ ] Використання QuestPDF або iTextSharp
- [ ] Шаблони звітів
- [ ] Генерація повного звіту справи
- [ ] Включення всіх даних діагностики
- [ ] Включення фото
- [ ] Експорт через API
- [ ] Тести

**Технічні деталі**:
- Шаблони для різних типів звітів
- Підтримка української та англійської мов
- Брендинг

**Підзадачі**:
1. Вибір бібліотеки для PDF
2. Створення базового шаблону
3. Генерація звіту WiFI
4. Генерація звіту CRAB
5. Генерація звіту 2YLE
6. Генерація звіту GLASS
7. Додавання фото
8. API endpoint для експорту

---

#### Task 2.1.2: Експорт в Word/Excel
**ID**: TASK-212  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 8  
**Оцінка часу**: 5-7 днів

**Опис**:
Додати можливість експорту в Word та Excel формати.

**Acceptance Criteria**:
- [ ] Експорт в Word (.docx)
- [ ] Експорт в Excel (.xlsx)
- [ ] Шаблони для Word/Excel
- [ ] API endpoints
- [ ] Тести

**Залежності**: TASK-211

---

### Epic 2.2: Історія змін

#### Task 2.2.1: Audit log для справ
**ID**: TASK-221  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 13  
**Оцінка часу**: 7-10 днів

**Опис**:
Реалізувати відстеження всіх змін в справах.

**Acceptance Criteria**:
- [ ] Audit log таблиця
- [ ] Автоматичне логування змін
- [ ] Відстеження хто/коли/що змінив
- [ ] API для отримання історії
- [ ] UI для відображення історії
- [ ] Можливість відкочення змін

**Технічні деталі**:
- Entity Framework Change Tracking
- Shadow properties для audit
- Soft delete

---

### Epic 2.3: Покращення AI

#### Task 2.3.1: Контекстні рекомендації AI
**ID**: TASK-231  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 13  
**Оцінка часу**: 7-10 днів

**Опис**:
Покращити AI асистента для надання контекстних рекомендацій на основі справи.

**Acceptance Criteria**:
- [ ] Передача контексту справи в AI
- [ ] Структуровані промпти
- [ ] Збереження історії чату
- [ ] Персоналізація рекомендацій
- [ ] Тестування якості рекомендацій

**Технічні деталі**:
- System prompts з контекстом
- Few-shot learning
- Context window management

---

## Фаза 3: Інтеграції

### Epic 3.1: HL7/FHIR інтеграція

#### Task 3.1.1: FHIR API реалізація
**ID**: TASK-311  
**Тип**: Development  
**Пріоритет**: Medium  
**Story Points**: 21  
**Оцінка часу**: 15-20 днів

**Опис**:
Реалізувати FHIR API для інтеграції з медичними системами.

**Acceptance Criteria**:
- [ ] FHIR сервер налаштовано
- [ ] Підтримка основних ресурсів (Patient, Observation, Condition)
- [ ] Імпорт даних з FHIR
- [ ] Експорт даних в FHIR
- [ ] Валідація FHIR ресурсів
- [ ] Документація

**Технічні деталі**:
- Hl7.Fhir.R4 або Firely SDK
- FHIR сервер або middleware
- Mapping між внутрішніми моделями та FHIR

---

## Фаза 4: Підприємницькі функції

### Epic 4.1: Мультитенантність

#### Task 4.1.1: Модель організацій
**ID**: TASK-411  
**Тип**: Development  
**Пріоритет**: Low  
**Story Points**: 21  
**Оцінка часу**: 15-20 днів

**Опис**:
Реалізувати підтримку багатьох організацій (tenants).

**Acceptance Criteria**:
- [ ] Tenant модель
- [ ] Ізоляція даних між tenants
- [ ] Tenant context middleware
- [ ] Управління tenants
- [ ] Брендинг для кожного tenant
- [ ] Тести

**Технічні деталі**:
- Tenant ID в кожній таблиці
- Global query filters
- Tenant resolution strategy

---

## Шаблони задач

### Шаблон User Story

```
**ID**: TASK-XXX
**Тип**: Development / Bug / Documentation / Testing
**Пріоритет**: Critical / High / Medium / Low
**Story Points**: X
**Оцінка часу**: X днів

**Опис**:
Короткий опис задачі

**Acceptance Criteria**:
- [ ] Критерій 1
- [ ] Критерій 2
- [ ] Критерій 3

**Технічні деталі**:
Деталі реалізації

**Залежності**: 
- TASK-XXX

**Підзадачі**:
1. Підзадача 1
2. Підзадача 2
```

### Шаблон Bug Report

```
**ID**: BUG-XXX
**Тип**: Bug
**Пріоритет**: Critical / High / Medium / Low
**Severity**: Critical / High / Medium / Low

**Опис**:
Опис бага

**Кроки відтворення**:
1. Крок 1
2. Крок 2

**Очікуваний результат**:
Що має статися

**Фактичний результат**:
Що сталося

**Environment**:
- Browser: 
- OS:
- Version:

**Screenshots/Logs**:
```

---

**Останнє оновлення**: 2025-01-XX  
**Версія документа**: 1.0

