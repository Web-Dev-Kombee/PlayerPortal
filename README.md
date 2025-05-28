
# 🧩 PlayerPortal – ASP.NET Core Web App

**PlayerPortal** is a clean, modular ASP.NET Core 8.0 web application implementing **Vanilla CQRS** (Command Query Responsibility Segregation). The application supports full **CRUD operations** for managing football players and includes features such as **model validation**, **search**, and **pagination**.

---

## 📁 Folder Structure

```
PlayerPrortal.sln
│
├───PlayerPortal/                        
│   ├───Controllers/                     # MVC Controller (PlayerController.cs)
│   ├───Helpers/                         # Global error handling (ErrorHandler.cs)
│   ├───Services/                        # IPlayerServices, PlayerService
│   ├───Views/                           # Razor views for Player UI
│   │   ├───Player/
│   │   └───Shared/
│   └───wwwroot/                         # Static assets (CSS, JS, images)
│
├───PlayerPortal.Data/                   
│   ├───BrokerRequests/                  # Brokers for CQRS logic
│   ├───DataTransferModels/              # DTOs (PlayerDataTransferModel)
│   ├───Extensions/                      # Extension methods
│   ├───Infrastructure/                 
│   │   ├───Tables/                      # PlayerTable.cs
│   │   └───EMDboDBContext.cs            # EF Core DbContext
│   ├───Migrations/                      # EF Migrations
│   └───SqlDb/                           # SQL Repositories (SqlDbRepository, SqlPlayerRepository)
│
├───Shard.Commons/                       # Common utilities
└───Shared.Broker/                       # Shared broker interfaces
```

---

## ✨ Key Features

- ✅ **Vanilla CQRS Pattern** – Manually wired Commands (writes) and Queries (reads). No MediatR.
- ✅ **CRUD Operations** – Full Create, Read, Update, Delete support for Player entities.
- ✅ **Search Functionality** – Search players by attributes such as Name, Goals, etc.
- ✅ **Pagination** – Index view supports paginated results with navigation.
- ✅ **Model Validation** – Server-side (Data Annotations) and client-side (jQuery + Razor).

---

## ⚙️ Core Components

### Commands (in `BrokerRequests/`)
- `PlayerRequest.cs` – Handles Create/Update logic
- `DeletePlayerBroker.cs` – Handles player deletion

### Queries (in `BrokerRequests/`)
- `PlayerListBroker.cs` – Retrieves filtered/paginated list of players
- `GetPlayerByIdBroker.cs` – Retrieves a single player by ID

### Services
- `IPlayerServices.cs` – Interface for player logic
- `PlayerService.cs` – Implements logic routing via brokers

### Data Layer
- `EMDboDBContext.cs` – EF Core DbContext
- `PlayerTable.cs` – Player entity model
- `SqlPlayerRepository.cs` – Actual DB operations

---

## 🧪 Validations

| Field | Rule |
|-------|------|
| Name  | Required, min length |
| Goals | Required, positive integer |

---

## 🔍 Search + Pagination

The `Index.cshtml` view in `Views/Player/` supports:
- Search input for filtering players
- Paginated results with query string support
- Pagination logic handled via `PlayerListBroker.cs`

---

## 🛠️ Technologies Used

- ASP.NET Core 8.0 (MVC)
- Entity Framework Core (Code First)
- Vanilla CQRS (no MediatR)
- Razor Views
- Bootstrap 5
- jQuery / JavaScript

---

## 🚀 Getting Started

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Web-Dev-Kombee/PlayerPortal.git
   ```

2. **Update `appsettings.Development.json`:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=PlayerDB;Trusted_Connection=True;"
   }
   ```

3. **Apply EF Migrations**
   ```bash
   Add-Migration InitialCreate
   Update-Database
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

---

## 📸 Screenshots (To Be Added)

- Add Player Form
![image](https://github.com/user-attachments/assets/2cbdce5b-37fd-44ff-8a55-a3c7ae13cba7)

- Player List with Pagination + Search
![image](https://github.com/user-attachments/assets/d5508de8-af4c-4c25-b3b1-ba8fe8c08047)

- Player Details
![image](https://github.com/user-attachments/assets/4b4c1fe1-eff0-4ada-b3cb-b2622418dd99)

- Edit Player
![image](https://github.com/user-attachments/assets/dbe90540-d29c-46b3-9fea-3595b8abdfe2)

- Delete Confirmation
![image](https://github.com/user-attachments/assets/e3d87f7f-cf26-4173-a22a-11ae77528d3a)


---

## 🤝 **Contributing**

We welcome contributions! Follow these steps to contribute:

1. Fork the repository.
2. Create a new branch for your feature/fix.
3. Commit changes and open a **Pull Request**.

---
   
## 🗝 License

This project is licensed under the [MIT License](LICENSE).

---
## 👨‍💻 **Author**

**Kombee Technologies**

- 🌐 [Portfolio](https://github.com/kombee-technologies)
- 💼 [LinkedIn](https://in.linkedin.com/company/kombee-global)
- 🌍 [Website](https://www.kombee.com/)

---

<p align="center">
  Built with ❤️ using .NET 
</p>

---
