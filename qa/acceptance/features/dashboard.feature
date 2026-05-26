@ui @dashboard
Feature: Visualización del dashboard
  Como usuario autenticado
  Quiero navegar por el dashboard
  Para revisar métricas académicas

  Scenario: Ver resumen general
    Given que he iniciado sesión correctamente
    When abro la sección resumen
    Then debo ver el título "Resumen General"

  Scenario: Navegar entre secciones
    Given que he iniciado sesión correctamente
    When cambio a la sección cursos
    Then debo ver el título "Estadísticas por Curso"