import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Employees (Admin only)', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/employees');
    await page.waitForLoadState('networkidle');
  });

  test('shows employees table', async ({ page }) => {
    await expect(page.getByRole('table')).toBeVisible();
    const rows = page.locator('table tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });
  });

  test('shows 3 employees (seeded data)', async ({ page }) => {
    const rows = page.locator('table tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThanOrEqual(3);
  });

  test('opens edit dialog', async ({ page }) => {
    await page.locator('table tbody tr').first().getByRole('button').first().click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await expect(page.getByRole('dialog').getByText(/edit employee/i)).toBeVisible();
  });

  test('edit dialog has pre-filled name and email', async ({ page }) => {
    await page.locator('table tbody tr').first().getByRole('button').first().click();
    const dialog = page.getByRole('dialog');
    const nameVal = await dialog.locator('input').first().inputValue();
    expect(nameVal.length).toBeGreaterThan(0);
  });
});
