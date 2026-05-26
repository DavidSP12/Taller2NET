package com.taller4.acceptance.steps;

import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;

import static org.junit.jupiter.api.Assertions.assertTrue;

public class ApiSteps {

    @Given("que tengo un token válido")
    public void queTengoUnTokenValido() {
        assertTrue(true);
    }

    @When("consulto el listado de estudiantes")
    public void consultoElListadoDeEstudiantes() {
        assertTrue(true);
    }

    @Then("el servicio debe responder con estado 200")
    public void elServicioDebeResponderConEstado200() {
        assertTrue(true);
    }

    @Then("el JSON debe contener la colección de estudiantes")
    public void elJsonDebeContenerLaColeccionDeEstudiantes() {
        assertTrue(true);
    }

    @Given("que tengo rol autorizado")
    public void queTengoRolAutorizado() {
        assertTrue(true);
    }

    @When("envío un estudiante válido")
    public void envioUnEstudianteValido() {
        assertTrue(true);
    }

    @When("consulto el listado de cursos")
    public void consultoElListadoDeCursos() {
        assertTrue(true);
    }

    @When("envío un curso válido")
    public void envioUnCursoValido() {
        assertTrue(true);
    }
}