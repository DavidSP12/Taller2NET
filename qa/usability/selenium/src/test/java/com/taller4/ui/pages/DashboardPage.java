package com.taller4.ui.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.TimeoutException;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

public class DashboardPage {
    private final WebDriver driver;
    private final WebDriverWait wait;

    public DashboardPage(WebDriver driver) {
        this.driver = driver;
        this.wait = new WebDriverWait(driver, Duration.ofSeconds(10));
    }

    public String sectionTitle() {
        return driver.findElement(By.id("sectionTitle")).getText();
    }

    public void openSection(String section) {
        driver.findElement(By.cssSelector("button[data-section='" + section + "']")).click();
    }

    public boolean isAppVisible() {
        try {
            wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("app")));
            return true;
        } catch (TimeoutException ex) {
            return false;
        }
    }

    public void waitForSectionTitle(String expected) {
        wait.until(ExpectedConditions.textToBe(By.id("sectionTitle"), expected));
    }
}