package com.taller4.ui;

import com.taller4.ui.pages.DashboardPage;
import com.taller4.ui.pages.LoginPage;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.assertEquals;

class DashboardUsabilityTest extends BaseUiTest {

    @Test
    void userShouldLoginAndNavigateSections() {
        LoginPage loginPage = new LoginPage(driver);
        DashboardPage dashboardPage = new DashboardPage(driver);

        loginPage.open(baseUrl);
        loginPage.login("admin", "Admin@123");

        assertTrue(dashboardPage.isAppVisible());
        dashboardPage.waitForSectionTitle("Resumen General");
        assertEquals("Resumen General", dashboardPage.sectionTitle());

        dashboardPage.openSection("courses");
        dashboardPage.waitForSectionTitle("Estadísticas por Curso");
        assertEquals("Estadísticas por Curso", dashboardPage.sectionTitle());

        dashboardPage.openSection("students");
        dashboardPage.waitForSectionTitle("Estadísticas por Estudiante");
        assertEquals("Estadísticas por Estudiante", dashboardPage.sectionTitle());
    }
}