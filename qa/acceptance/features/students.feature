@api @students
Feature: Gestión de estudiantes
  Como docente o administrador
  Quiero consultar y administrar estudiantes
  Para mantener actualizada la información académica

  Scenario: Consultar listado de estudiantes
    Given que tengo un token válido
    When consulto el listado de estudiantes
    Then el servicio debe responder con estado 200
    And el JSON debe contener la colección de estudiantes

  Scenario: Crear un estudiante con datos válidos
    Given que tengo rol autorizado
    When envío un estudiante válido
    Then el servicio debe responder con 201 o 200 según la implementación