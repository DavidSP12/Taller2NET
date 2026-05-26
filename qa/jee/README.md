# Pruebas técnicas JEE

No existe un módulo Java/JEE en este workspace. Este paquete se entrega como plantilla para un backend REST equivalente usando Jakarta EE o JAX-RS, con JUnit 5, Mockito y REST Assured.

## Qué incluye

- Integración/API con REST Assured.
- Pruebas de servicio con Mockito.
- Casos de éxito, error y validación JSON.

## Suposición de endpoints

La plantilla usa una API REST con estos recursos:

- `/api/auth/login`
- `/api/students`
- `/api/students/{id}`
- `/api/courses`
- `/api/courses/{id}`
- `/api/dashboard/summary`

## Adaptación

Si tu taller JEE usa Spring Boot en vez de Jakarta EE, conserva la estructura y cambia la capa HTTP por `MockMvc`.