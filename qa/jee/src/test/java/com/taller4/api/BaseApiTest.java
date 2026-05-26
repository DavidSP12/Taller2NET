package com.taller4.api;

import io.restassured.RestAssured;
import org.junit.jupiter.api.BeforeAll;

public abstract class BaseApiTest {
    @BeforeAll
    static void setup() {
        RestAssured.baseURI = System.getenv().getOrDefault("BASE_URL", "http://localhost");
        RestAssured.basePath = "";
    }
}