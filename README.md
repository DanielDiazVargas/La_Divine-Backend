# 🎭 BACKEND - Sistema Web Teatro La Divine

## 📌 Descripción del proyecto

API REST para el sistema de gestión del Teatro La Divine. Este módulo expone los endpoints que permiten autenticar usuarios, gestionar cuentas, administrar funciones y procesar reservas de entradas.

Proyecto desarrollado en el contexto de la asignatura **Ingeniería de Software**.

---

## ⚙️ Cómo ejecutar el proyecto

### Prerrequisitos
- .NET 8 SDK
- Acceso a internet (la base de datos está alojada en Neon - PostgreSQL en la nube)

### Pasos

1. Abre una terminal y posiciónate dentro de la carpeta `backend`.
2. Crea el archivo `appsettings.json` en la raíz del proyecto con la siguiente estructura y completa los valores:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...; Database=...; Username=...; Password=...; SSL Mode=VerifyFull; Channel Binding=Require;"
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "LaDivineApp",
    "Audience": "LaDivineUsers",
    "ExpireMinutes": 120
  },
  "Cloudinary": {
    "CloudName": "...",
    "ApiKey": "...",
    "ApiSecret": "..."
  },
  "SendGrid": {
    "ApiKey": "...",
    "FromEmail": "...",
    "FromName": "Teatro La Divine"
  },
  "TeatroConfig": {
    "AdminEmail": "..."
  }
}
```

3. Restaura las dependencias:
```
dotnet restore
```

4. Levanta el servidor:
```
dotnet run
```

5. La API estará disponible en `http://localhost:5XXX`. La documentación interactiva (Swagger) estará en:
```
http://localhost:5XXX/swagger
```
> Swagger solo está disponible en entorno de desarrollo (`Development`).

---

## 🌐 CORS configurado

El backend acepta solicitudes desde los siguientes orígenes:

| Origen | Descripción |
| :--- | :--- |
| `http://localhost:5173` | Frontend en desarrollo local (Vite) |
| `https://divinecommit-frontend.onrender.com` | Frontend desplegado en producción |

---

## 👑 Endpoints implementados (Iteración 1)

### 🔐 Auth — `/api/auth`

| Método | Ruta | Rol | Descripción |
| :--- | :--- | :--- | :--- |
| POST | `/api/auth/login` | Público | Inicia sesión y retorna un JWT |
| POST | `/api/auth/register` | Público | Registra un nuevo cliente |
| PATCH | `/api/auth/change-password` | User, Admin | Cambia la contraseña del usuario autenticado |

### 👤 User — `/api/user`

| Método | Ruta | Rol | Descripción |
| :--- | :--- | :--- | :--- |
| GET | `/api/user` | Admin | Lista todos los usuarios del sistema |
| GET | `/api/user/search?email=...` | Admin | Busca un usuario por correo electrónico |
| PATCH | `/api/user/{id}/status` | Admin | Habilita o deshabilita un usuario |

---

## 🎟️ Endpoints implementados (Iteración 2)

### 🎭 Function — `/api/function`

| Método | Ruta | Rol | Descripción |
| :--- | :--- | :--- | :--- |
| POST | `/api/function` | Admin | Registra una nueva función (FUN-001). Recibe `multipart/form-data` para la imagen de portada |
| GET | `/api/function` | Admin | Lista todas las funciones registradas |
| GET | `/api/function/cartelera?name=&date=` | User, Admin | Retorna funciones vigentes con filtro opcional por nombre y fecha (FUN-002) |

### 🎫 Reservation — `/api/reservation`

| Método | Ruta | Rol | Descripción |
| :--- | :--- | :--- | :--- |
| GET | `/api/reservation/seats/{functionId}` | User, Admin | Retorna los asientos de una función con su estado de disponibilidad (FUN-003) |
| POST | `/api/reservation` | User | Crea una reserva con los asientos seleccionados para el usuario autenticado (FUN-003) |
| GET | `/api/reservation/my-reservations` | User | Retorna el historial de reservas del usuario autenticado (FUN-004) |

> Todos los endpoints de reserva obtienen el identificador del usuario desde el token JWT, sin necesidad de enviarlo en el cuerpo de la petición.

---

## 🏗️ Tecnologías y Estructura

* **Framework:** ASP.NET Core (C#)
* **Arquitectura:** MVC (Controllers / Services / Models)
* **Base de datos:** PostgreSQL en la nube (Neon), accedida con Entity Framework Core
* **Almacenamiento de imágenes:** Cloudinary
* **Envío de correos:** SendGrid
* **Autenticación:** JWT Bearer (expiración: 120 minutos)

Estructura de carpetas:

```
backend/
│
├── src/
│   ├── Controller/        # Controladores REST
│   │   ├── AuthController.cs
│   │   ├── FunctionController.cs
│   │   ├── ReservationController.cs
│   │   └── UserController.cs
│   ├── Db/                # Contexto de base de datos (Entity Framework Core) y Seeder
│   ├── Dtos/              # Objetos de transferencia de datos
│   │   ├── Function/      # CreateFunction, BillboardFunctionDto
│   │   ├── Reservation/   # ReservationDto, CreateReservation, etc.
│   │   ├── Seat/          # SeatDto
│   │   └── Users/
│   ├── Model/             # Modelos de dominio (Function, Reservation, Role, Seat, User)
│   ├── Services/          # Lógica de negocio
│   │   ├── Interfaces/    # IAuthServices, ICloudinaryServices, IFunctionServices,
│   │   │                  # IReservationServices, ISendGridEmailServices
│   │   ├── AuthServices.cs
│   │   ├── CloudinaryServices.cs
│   │   ├── FunctionServices.cs
│   │   ├── ReservationServices.cs
│   │   └── SendGridEmailServices.cs
│   └── Utils/
│       └── GenerateToken.cs   # Generación de JWT
└── Properties/
```

---

## 💻 Estándares de programación

### 🔹 Idioma
* **Código fuente:** En **Inglés** (controladores, servicios, modelos, variables, rutas).
  > ⚠️ Se reconoce que existen algunos deslices puntuales en español dentro del código fuente (principalmente en nombres de DTOs), los cuales no afectan el funcionamiento del sistema y serán corregidos en futuras iteraciones.
* **Documentación:** El README.md y comentarios en **Español**.

### 🔹 Nombres
* Clases, interfaces y DTOs: PascalCase
* Variables locales y parámetros: camelCase
* Interfaces: prefijo `I` (ej.: `IAuthServices`, `IFunctionServices`)

---

## 🧪 Casos de prueba Backend

| Caso | Endpoint | Resultado esperado |
| :--- | :--- | :--- |
| Credenciales incorrectas | POST `/api/auth/login` | 401 con mensaje de error |
| Token ausente o inválido | Cualquier endpoint protegido | 401 Unauthorized |
| Correo no registrado | GET `/api/user/search?email=...` | Mensaje: "El correo electrónico ingresado no se encuentra registrado" |
| Fecha de función pasada | POST `/api/function` | Mensaje: "La fecha y hora de la función no puede ser anterior a la fecha actual" |
| Conflicto de día | POST `/api/function` | Mensaje: "Ya existe una función registrada para este día" |
| Asiento no disponible | POST `/api/reservation` | Mensaje: "El asiento seleccionado no se encuentra disponible" |
| Sin reservas | GET `/api/reservation/my-reservations` | Mensaje: "No hay reservas asociadas a su usuario" |

---

## 👥 Integrantes

* Martín Alejandro Solari Garrido
* Daniel Antonio Días Vargas
* Gianny Alfredo Paredes Araya
* Juan José Gómez Vergara

---

## 📌 Estado del proyecto
✅ **Iteración 1 Finalizada** – Autenticación, gestión de usuarios y control de acceso implementados.  
✅ **Iteración 2 Finalizada** – Registro de funciones, cartelera, reserva y consulta de reservas integrados.
