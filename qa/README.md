# Taller 4 - QA

Este bloque concentra la solución integral del Taller 4 sobre el proyecto real existente: .NET 8 + frontend web. No se detectó ningún módulo Java/JEE en el workspace, por lo que el punto 2 se entrega como plantilla funcional basada en JAX-RS/Jakarta EE para un backend REST equivalente.

## Estructura sugerida

```text
qa/
  performance/
    k6/
      config.js
      availability-test.js
      load-test.js
      stress-test.js
      README.md
    jmeter/
      Taller4-TestPlan.md
  jee/
    README.md
    pom.xml
    src/test/java/com/taller4/api/
      BaseApiTest.java
      AuthResourceIT.java
      StudentsResourceIT.java
      CoursesResourceIT.java
      DashboardResourceIT.java
    src/test/java/com/taller4/service/
      AuthServiceTest.java
      StudentServiceTest.java
      CourseServiceTest.java
  usability/
    README.md
    selenium/
      pom.xml
      src/test/java/com/taller4/ui/
        BaseUiTest.java
        pages/
          LoginPage.java
          DashboardPage.java
        DashboardUsabilityTest.java
      heuristic-checklist.md
  acceptance/
    README.md
    features/
      auth.feature
      students.feature
      courses.feature
      dashboard.feature
    steps/
      ApiSteps.java
      UiSteps.java
    pom.xml
```

## Cobertura incluida

- `performance/`: disponibilidad, carga, estrés y plan de JMeter.
- `jee/`: pruebas de integración y API con JUnit 5, Mockito y REST Assured.
- `usability/`: Selenium para la UI del dashboard y checklist heurística donde aplique.
- `acceptance/`: escenarios Gherkin y estructura base de step definitions.

## Nota de alcance

- El frontend existente en el repo es un dashboard web en HTML/CSS/JS con login JWT y navegación por secciones.
- No hay UI separada para JEE ni código Java en el workspace.
- Por eso, el material JEE se entrega como plantilla lista para adaptar a un proyecto Java REST equivalente, y la usabilidad se orienta a la UI real del dashboard .NET.