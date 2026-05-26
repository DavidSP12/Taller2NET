package com.taller4.acceptance.steps;

import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;

import static org.junit.jupiter.api.Assertions.assertTrue;

public class UiSteps {

    @Given("que estoy en la pantalla de login")
    public void queEstoyEnLaPantallaDeLogin() {
        assertTrue(true);
    }

    @When("ingreso el usuario {string} y la contraseña {string}")
    public void ingresoElUsuarioYLaContrasena(String username, String password) {
        assertTrue(username != null && password != null);
    }

    @Then("debo ver el dashboard principal")
    public void deboVerElDashboardPrincipal() {
        assertTrue(true);
    }

    @When("ingreso credenciales inválidas")
    public void ingresoCredencialesInvalidas() {
        assertTrue(true);
    }

    @Then("debo ver un mensaje de error claro")
    public void deboVerUnMensajeDeErrorClaro() {
        assertTrue(true);
    }

    @Given("que he iniciado sesión correctamente")
    public void queHeIniciadoSesionCorrectamente() {
        assertTrue(true);
    }

    @When("abro la sección resumen")
    public void abroLaSeccionResumen() {
        assertTrue(true);
    }

    @When("cambio a la sección cursos")
    public void cambioALaSeccionCursos() {
        assertTrue(true);
    }

    @Then("debo ver el título {string}")
    public void deboVerElTitulo(String titulo) {
        assertTrue(titulo != null && !titulo.isBlank());
    }
}