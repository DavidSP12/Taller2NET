# Taller 4 - JMeter Test Plan

Este documento describe el plan de pruebas para disponibilidad, carga, estrés y estabilidad del backend REST del proyecto.

## 1. Objetivo

Validar que el sistema mantenga tiempos de respuesta aceptables y no caiga ante concurrencia, carga sostenida y picos de estrés.

## 2. Alcance

Prioridad alta para estos endpoints:

- `GET /health`
- `POST /api/auth/login`
- `GET /api/dashboard/summary`
- `GET /api/dashboard/top-courses?top=N`
- `GET /api/students`
- `GET /api/courses`

Si el backend JEE equivalente expone CRUD REST, priorizar también:

- `GET /api/auth/health`
- `GET /api/students/{id}`
- `POST /api/students`
- `PUT /api/students/{id}`
- `DELETE /api/students/{id}`
- `GET /api/courses/{id}`

## 3. Métricas esperadas

### Disponibilidad

- Tasa de error menor al 1% en carga normal.
- Sin errores 5xx en endpoints críticos durante la prueba base.
- El endpoint `health` debe responder 200 durante toda la ejecución.

### Escalabilidad

- `p95` menor a 900 ms en disponibilidad.
- `p95` menor a 1200 ms en carga.
- `p99` menor a 1800 ms en carga.
- `p95` menor a 2500 ms en estrés.
- Throughput estable sin degradación abrupta entre rampas.

### Concurrencia

- El sistema debe aceptar múltiples sesiones autenticadas concurrentes.
- No debe presentar corrupción de respuestas JSON.
- Las respuestas deben mantener el esquema esperado aun bajo presión.

## 4. Escenarios de prueba

### 4.1 Carga

Propósito: medir comportamiento con concurrencia sostenida.

Configuración sugerida:

- `Thread Group` con rampa de 1 a 50 usuarios.
- `Ramp-Up`: 60 a 180 segundos.
- `Loop Count`: continuo durante 5 a 10 minutos.
- `Timers`: Constant Timer de 1 a 3 s entre solicitudes.

### 4.2 Estrés

Propósito: identificar el punto de quiebre y el modo de degradación.

Configuración sugerida:

- Rampa progresiva de 25, 75, 150 y 250 usuarios.
- Duración total: 6 a 8 minutos.
- Observación de errores, latencia y caídas del gateway o del servicio.

### 4.3 Estabilidad / Soak

Propósito: validar comportamiento continuo y caché.

Configuración sugerida:

- 10 a 20 usuarios.
- Duración de 30 a 60 minutos.
- Validar que `dashboard/summary` y `dashboard/top-courses` sirvan datos desde Redis cuando aplique.

## 5. Plan de JMeter

### Test Plan

- `Thread Group` principal para usuarios autenticados.
- `HTTP Request Defaults` con host del gateway y protocolo `http`.
- `HTTP Header Manager` con `Content-Type: application/json` y `Authorization: Bearer ${token}`.
- `HTTP Cookie Manager` habilitado si la sesión lo requiere.
- `HTTP Cache Manager` para medir comportamiento real del navegador o desactivar si se busca carga pura de API.

### Samplers

1. `POST /api/auth/login`
2. `GET /health`
3. `GET /api/dashboard/summary`
4. `GET /api/dashboard/top-courses?top=5`
5. `GET /api/students?page=1&pageSize=10`
6. `GET /api/courses?page=1&pageSize=10`

### Assertions

- `Response Assertion` para validar HTTP 200 o 201 según endpoint.
- `JSON Assertion` o `JSON JMESPath Assertion` para verificar `data`, `success` o `Status`.
- `Duration Assertion` opcional para SLA puntual.

### Listeners recomendados

- `Summary Report`
- `Aggregate Report`
- `Response Time Graph`
- `View Results Tree` solo en debugging, no en carga real.
- `Backend Listener` si se desea exportar a InfluxDB/Grafana.

## 6. Plantilla de variables

- `BASE_URL`: `http://localhost`
- `USERNAME`: `admin`
- `PASSWORD`: `Admin@123`

## 7. Criterios de éxito

- Todas las peticiones críticas responden 200/201 según corresponda.
- El error rate no supera el umbral definido.
- El `p95` cumple el SLA.
- El sistema degrada de manera progresiva, no colapsa abruptamente.

## 8. Mapeo al proyecto real

- Gateway: puerto 80.
- Academic Service: puerto 8080.
- Statistics Service: puerto 8081.
- Frontend: puerto 3000.
