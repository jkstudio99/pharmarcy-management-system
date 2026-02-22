import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Suppliers (Admin only)', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/suppliers');
    await page.waitForLoadState('networkidle');
  });

  test('shows suppliers table', async ({ page }) => {
    await expect(page.getByRole('table')).toBeVisible();
    const rows = page.locator('table tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });
  });

  test('shows Bangkok Drug Co. in list', async ({ page }) => {
    await expect(page.getByText(/Bangkok Drug/i)).toBeVisible({ timeout: 8_000 });
  });

  test('opens add supplier dialog', async ({ page }) => {
    await page.getByRole('button', { name: /add supplier/i }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
  });

  test('creates a new supplier', async ({ page }) => {
    const name = `Test Supplier ${Date.now()}`;
    await page.getByRole('button', { name: /add supplier/i }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByPlaceholder(/supplier name|e\.g\./i).fill(name);
    await dialog.getByRole('button', { name: /create/i }).click();
    await expect(page.getByText(/success/i).first()).toBeVisible({ timeout: 8_000 });
    await expect(page.getByText(name)).toBeVisible({ timeout: 8_000 });
  });

  test('search filters suppliers', async ({ page }) => {
    await page.getByPlaceholder(/search/i).fill('Bangkok');
    await page.keyboard.press('Enter');
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(/Bangkok Drug/i)).toBeVisible();
  });
});
