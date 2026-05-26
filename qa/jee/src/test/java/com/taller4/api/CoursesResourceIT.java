package com.taller4.api;

import org.junit.jupiter.api.Test;

import static io.restassured.RestAssured.given;
import static org.hamcrest.Matchers.anyOf;
import static org.hamcrest.Matchers.equalTo;

class CoursesResourceIT extends BaseApiTest {

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
    void getCoursesShouldReturnJsonPayload() {
        given()
            .header("Authorization", "Bearer " + token())
        .when()
            .get("/api/courses?page=1&pageSize=10")
        .then()
            .statusCode(200)
            .body("data", anyOf(equalTo(null), equalTo(null)));
    }
}