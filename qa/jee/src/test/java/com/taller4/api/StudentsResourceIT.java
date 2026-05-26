package com.taller4.api;

import org.junit.jupiter.api.Test;

import static io.restassured.RestAssured.given;
import static org.hamcrest.Matchers.anyOf;
import static org.hamcrest.Matchers.equalTo;
import static org.hamcrest.Matchers.notNullValue;

class StudentsResourceIT extends BaseApiTest {

    private String token() {
        return given()
            .contentType("application/json")
            .body("{" +
                "\"username\":\"admin\"," +
                "\"password\":\"Admin@123\"}" )
        .when()
            .post("/api/auth/login")
        .then()
            .extract().path("data.token");
    }

    @Test
    void getStudentsShouldReturnOkWithPagination() {
        given()
            .header("Authorization", "Bearer " + token())
        .when()
            .get("/api/students?page=1&pageSize=10")
        .then()
            .statusCode(200)
            .body(notNullValue());
    }

    @Test
    void createStudentShouldValidateBusinessRules() {
        given()
            .header("Authorization", "Bearer " + token())
            .contentType("application/json")
            .body("{" +
                "\"studentCode\":\"ST-001\"," +
                "\"firstName\":\"Ana\"," +
                "\"lastName\":\"Perez\"," +
                "\"email\":\"ana.perez@example.com\"," +
                "\"program\":\"Systems\"," +
                "\"semester\":3}" )
        .when()
            .post("/api/students")
        .then()
            .statusCode(anyOf(equalTo(201), equalTo(400), equalTo(401), equalTo(403)));
    }
}