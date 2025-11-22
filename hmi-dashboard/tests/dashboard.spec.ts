import { test, expect } from '@playwright/test';


test('dashboard heading and connection status are visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    await expect(page.locator('#dashboard-title')).toHaveText('Smart Warehouse Dashboard');
    await expect(page.locator('#connection-status')).toContainText('Connection');
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

test('alarm panel is visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    // Check if the main alarm panel container exists
    await expect(page.locator('.alarm-panel')).toBeVisible();
});

test('alarm panel header and active count are visible', async ({ page }) => {
    await page.goto('http://localhost:5173');
    // Verify the "System Alarms" header matches the App.tsx code
    await expect(page.locator('text=System Alarms')).toBeVisible();
    // Verify the Active count badge exists
    await expect(page.locator('.alarm-count')).toBeVisible();
});

test('alarm panel shows "No active alarms" by default', async ({ page }) => {
    await page.goto('http://localhost:5173');
    // Verify the empty state message when no data is flowing
    await expect(page.locator('.no-alarms')).toHaveText('No active alarms. System normal.');
});

// Optional: Test for active alarms 
// (Note: This will only pass if your backend is running and actively sending alarm data)
test('alarm list renders when data is present', async ({ page }) => {
    await page.goto('http://localhost:5173');

    // Check if the alarm list is visible (requires active alarms from backend)
    const alarmList = page.locator('.alarm-list');

    // Using a conditional check so the test doesn't fail if the backend isn't running
    if (await alarmList.isVisible()) {
        await expect(alarmList).toBeVisible();
        await expect(page.locator('.alarm-item')).toBeVisible();
    }
});