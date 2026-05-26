package com.taller4.api;

import org.junit.jupiter.api.Test;

import static io.restassured.RestAssured.given;
import static org.hamcrest.Matchers.anyOf;
import static org.hamcrest.Matchers.equalTo;
import static org.hamcrest.Matchers.notNullValue;

class AuthResourceIT extends BaseApiTest {

    @Test
    void loginShouldReturnTokenForValidCredentials() {
        given()
            .contentType("application/json")
            .body("{" +
                "\"username\":\"admin\"," +
                "\"password\":\"Admin@123\"}" )
        .when()
            .post("/api/auth/login")
        .then()
            .statusCode(anyOf(equalTo(200), equalTo(201)))
            .body("data.token", anyOf(notNullValue(), notNullValue()));
    }

    @Test
    void loginShouldRejectInvalidCredentials() {
        given()
            .contentType("application/json")
            .body("{" +
                "\"username\":\"invalid\"," +
                "\"password\":\"wrong\"}" )
        .when()
            .post("/api/auth/login")
        .then()
            .statusCode(401)
            .body("success", equalTo(false));
    }
}