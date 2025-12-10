import { test, expect } from '@playwright/test';

// Define the base URL for your Vite/React app
test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5173/');

    // System must be fully running before tests start
    // Check for "Connected" status in the status bar
    await expect(page.getByText('Connection: Connected')).toBeVisible({ timeout: 10000 });
});

test.describe('E2E System Functionality Tests', () => {

    test('E2E 1: Start/Stop Cycle and Live Lane Flow', async ({ page }) => {
        // 1. Start the Conveyor
        await page.getByRole('button', { name: 'START' }).click();

        // Check that the status is running
        await expect(page.getByText('Conveyor: RUNNING')).toBeVisible();

        // 2. Verify Packages start flowing into the lanes (Express/Standard/Heavy)
        // Since packages disappear after 3s, we check if a package appears at all.
        const lane1 = page.locator('.lane-box.express .package-slot.occupied');
        const lane2 = page.locator('.lane-box.standard .package-slot.occupied');

        // Wait for a package to show up in Lane 1 or Lane 2
        await expect.soft(lane1.or(lane2)).toBeVisible({ timeout: 5000 });

        // 3. Stop the Conveyor
        await page.getByRole('button', { name: 'STOP' }).click();

        // Check that the status is stopped
        await expect(page.getByText('Conveyor: STOPPED')).toBeVisible();

        // 4. Verify package flow has stopped
        // The last package displayed should freeze (it won't be replaced by a new one)
        // Note: The package on screen may disappear after 3s (its timer), but no new one will appear.
        // We ensure the "Empty" state eventually returns if the timer runs out, but no "occupied" state follows.
        await expect(page.getByText('Empty').nth(0)).toBeVisible({ timeout: 4000 });
    });

    test('E2E 2: Emergency Stop Activation and Lane Blocking', async ({ page }) => {
        // Start the system first
        await page.getByRole('button', { name: 'START' }).click();
        await expect(page.getByText('Conveyor: RUNNING')).toBeVisible();

        // 1. Trigger E-Stop from the UI
        await page.getByRole('button', { name: 'Simulate E-Stop' }).click();

        // 2. Verify motor status is STOPPED
        await expect(page.getByText('Conveyor: STOPPED')).toBeVisible();

        // 3. Verify lanes are visually blocked
        const blockedLane = page.locator('.package-slot.blocked-lane').first();
        await expect(blockedLane).toHaveText('⛔ E-STOP');

        // 4. Verify High Severity Alarm is raised
        const highAlarm = page.locator('.alarm-item.high');
        await expect(highAlarm).toContainText('E-STOP ACTIVATED');

        // 5. Test Alarm Acknowledge
        await highAlarm.click();
        await page.getByRole('button', { name: 'Acknowledge' }).click();

        // Verify alarm count decreases (should be 0 if this was the only alarm)
        await expect(page.locator('.alarm-count')).not.toContainText('1');

        // 6. Release E-Stop and verify restart (Auto-restart is in C# Program.cs)
        await page.getByRole('button', { name: 'Simulate E-Stop' }).click();
        await expect(page.getByText('Conveyor: RUNNING')).toBeVisible({ timeout: 5000 });

        // Packages should now be flowing (Lanes should not show blocked status)
        await expect(page.locator('.package-slot.blocked-lane')).toHaveCount(0);
    });

    test('E2E 3: Speed Synchronization and Chart Update', async ({ page }) => {
        // 1. Start Conveyor
        await page.getByRole('button', { name: 'START' }).click();

        const speedLabel = page.getByLabel('Speed:');

        // 2. Set speed low (e.g., 20 RPM)
        await page.getByRole('slider').fill('20');
        await expect(speedLabel).toContainText('Speed: 20');

        // 3. Set speed high (e.g., 90 RPM)
        await page.getByRole('slider').fill('90');
        await expect(speedLabel).toContainText('Speed: 90');

        // 4. Verify Chart updates (Checking the data point exists)
        // This validates: HMI -> Socket -> C# -> Socket -> HMI (Chart Data)
        await expect(page.locator('.recharts-surface')).toBeVisible();
    });

    test('E2E 4: Energy Report Generation Flow', async ({ page }) => {
        // Ensure system has collected data
        await page.getByRole('button', { name: 'START' }).click();
        await page.waitForTimeout(1000); // Wait for a few samples

        // 1. Click Generate Report button
        const reportButton = page.getByRole('button', { name: 'Generate Report' });

        // We spy on the global alert function, which the HMI uses to show the report confirmation
        const alertPromise = page.waitForEvent('alert');

        await reportButton.click();

        // 2. Wait for the C# backend to process and emit the event, triggering the alert on the HMI
        const alert = await alertPromise;

        // 3. Verify the alert message contains the expected file name pattern
        expect(alert.message()).toContain('Energy Report Generated Successfully!');
        expect(alert.message()).toContain('File saved as: energy_report_');

        await alert.dismiss();
    });
});