# Pruebas de rendimiento con k6

Estos scripts cubren el punto 1 del Taller 4 para el backend actual (.NET).

## Scripts

- `availability-test.js`: valida disponibilidad de los endpoints críticos y una sesión autenticada.
- `load-test.js`: simula carga concurrente sostenida para medir escalabilidad.
- `stress-test.js`: incrementa la presión hasta un nivel de estrés para observar degradación y estabilidad.

## Requisitos

- k6 instalado localmente, o Docker para ejecutarlo en contenedor.

## Variables de entorno

- `BASE_URL`: URL base del gateway. Por defecto usa `http://localhost`.
- `USERNAME`: usuario para autenticación. Por defecto usa `admin`.
- `PASSWORD`: contraseña para autenticación. Por defecto usa `Admin@123`.
- `VUS`: solo para `availability-test.js`.
- `DURATION`: solo para `availability-test.js`.

## Ejemplos de ejecución

```bash
k6 run -e BASE_URL=http://localhost qa/performance/k6/availability-test.js
k6 run -e BASE_URL=http://localhost qa/performance/k6/load-test.js
k6 run -e BASE_URL=http://localhost qa/performance/k6/stress-test.js
```

Con Docker:

```bash
docker run --rm -i -v "$PWD:/src" grafana/k6 run -e BASE_URL=http://localhost /src/qa/performance/k6/load-test.js
```