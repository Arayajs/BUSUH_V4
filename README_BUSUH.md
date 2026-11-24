# BUSUH — Reserva de boletos de bus

Sistema distribuido de reserva de boletos hecho con **C# / .NET 8 y WinForms**. Está compuesto por tres aplicaciones independientes que se comunican entre sí por **sockets TCP** usando un protocolo de tramas de longitud fija.

Proyecto académico. Por requisito del enunciado, los datos se guardan **en memoria** (arreglos de objetos), sin archivos ni base de datos.

<!-- Agrega aquí 2 o 3 capturas de pantalla: ![Cliente](docs/cliente.png) -->

## Arquitectura

| Aplicación | Rol |
|---|---|
| **BUSUH_Terminales** | Administra las terminales y las expone al servidor por TCP (puerto `30001`). |
| **BUSUH_Servidor** | Administra los viajes y atiende las reservas de los clientes. Un hilo por cliente y acceso sincronizado con `lock`. |
| **BUSUH_Cliente** | Muestra el mapa de asientos y envía la solicitud de boletos al servidor (puerto `30000`). |

```
Cliente  --TCP :30000-->  Servidor  --TCP :30001-->  Terminales
```

Cada aplicación sigue la misma estructura en capas (`Views → Controllers → Services → Repositories`, más `Models` y `DTOs`) y se apoya en interfaces para aplicar los principios SOLID, en especial SRP y DIP.

## Protocolo de tramas

**Solicitud (cliente → servidor), 3 caracteres:** `VVB`

| Campo | Largo | Ejemplo |
|---|---|---|
| Código de viaje | 2 | `01` |
| Cantidad de boletos | 1 | `3` |

`013` pide 3 boletos para el viaje 01.

**Respuesta (servidor → cliente), 24 caracteres:**

| Campo | Largo |
|---|---|
| Verificador | 1 |
| Código de viaje | 2 |
| Número de fila 1 | 2 |
| Asientos fila 1 | 5 |
| Número de fila 2 | 2 |
| Asientos fila 2 | 5 |
| Monto | 5 |
| Capacidad disponible | 2 |

Valores del verificador: `1` reserva exitosa, `0` sin asientos, `3` excede la cantidad permitida, `4` el viaje no existe, `5` formato inválido.

## Requisitos

- Windows (las tres aplicaciones usan WinForms)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Cómo ejecutarlo

Cada aplicación tiene su propia solución. Se recomienda iniciarlas en este orden:

```bash
cd BUSUH/Terminales && dotnet run     # 1. expone las terminales (puerto 30001)
cd BUSUH/Servidor   && dotnet run     # 2. atiende a los clientes (puerto 30000)
cd BUSUH/Cliente    && dotnet run     # 3. abre uno o varios clientes
```

Puedes abrir varios clientes a la vez para ver cómo el servidor atiende solicitudes concurrentes.

## Qué demuestra este proyecto

- Comunicación entre procesos con `TcpListener` / `TcpClient`.
- Servidor multihilo con sincronización de recursos compartidos.
- Diseño de un protocolo de mensajes de longitud fija, con validación y códigos de error.
- Separación en capas y programación contra interfaces.
