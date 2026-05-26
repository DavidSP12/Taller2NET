@api @courses
Feature: Gestión de cursos
  Como docente o administrador
  Quiero consultar y administrar cursos
  Para ofrecer una programación académica correcta

  Scenario: Consultar listado de cursos
    Given que tengo un token válido
    When consulto el listado de cursos
    Then el servicio debe responder con estado 200

  Scenario: Crear un curso con datos válidos
    Given que tengo rol autorizado
    When envío un curso válido
    Then el servicio debe responder con 201 o 200 según la implementación