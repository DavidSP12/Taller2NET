@auth @smoke
Feature: Autenticación del sistema
  Como usuario del sistema
  Quiero iniciar sesión y cerrar sesión
  Para acceder de forma segura al dashboard

  Scenario Outline: Inicio de sesión exitoso
    Given que estoy en la pantalla de login
    When ingreso el usuario "<username>" y la contraseña "<password>"
    Then debo ver el dashboard principal

    Examples:
      | username | password      |
      | admin    | Admin@123     |
      | teacher01| Teacher@123    |

  Scenario: Inicio de sesión fallido
    Given que estoy en la pantalla de login
    When ingreso credenciales inválidas
    Then debo ver un mensaje de error claro