import { test, expect } from '@playwright/test';

test('dashboard heading and connection status are visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    await expect(page.locator('#dashboard-title')).toHaveText('Smart Warehouse Dashboard');
    await expect(page.locator('#connection-status')).toContainText('Connection Status');
});

test('conveyor status is visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    await expect(page.locator('#conveyor-status')).toContainText('Conveyor');
});

test('start and stop buttons are visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    await expect(page.locator('text=Start Conveyor')).toBeVisible();
    await expect(page.locator('text=Stop Conveyor')).toBeVisible();
});

test('speed slider is visible and has default value', async ({ page }) => {
    await page.goto('http://localhost:5173');
    const slider = page.locator('#speed-slider');
    await expect(slider).toBeVisible();
    await expect(slider).toHaveValue('50');
});

test('chart container is visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    await expect(page.locator('#chart-container')).toBeVisible();
});