package com.taller4.ui;

import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;

import java.nio.file.Files;
import java.nio.file.Path;

public abstract class BaseUiTest {
    protected WebDriver driver;
    protected String baseUrl;

    @BeforeEach
    void openBrowser() {
        baseUrl = System.getenv().getOrDefault("UI_BASE_URL", "http://localhost:3000");
        ChromeOptions options = new ChromeOptions();
        String chromeBin = pickBinary(
            System.getenv("CHROME_BIN"),
            "/home/codespace/.cache/selenium/chrome/linux64/149.0.7827.22/chrome",
            "/usr/bin/chromium"
        );
        String chromeDriverBin = pickBinary(
            System.getenv("CHROMEDRIVER_BIN"),
            "/home/codespace/.cache/selenium/chromedriver/linux64/149.0.7827.22/chromedriver",
            "/usr/bin/chromedriver"
        );
        System.setProperty("webdriver.chrome.driver", chromeDriverBin);
        options.setBinary(chromeBin);
        options.addArguments(
            "--headless=new",
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--window-size=1280,720"
        );
        driver = new ChromeDriver(options);
        driver.manage().window().maximize();
    }

    private String pickBinary(String envValue, String primary, String fallback) {
        if (envValue != null && !envValue.isBlank()) {
            return envValue;
        }
        if (Files.exists(Path.of(primary))) {
            return primary;
        }
        return fallback;
    }

    @AfterEach
    void closeBrowser() {
        if (driver != null) {
            driver.quit();
        }
    }
}