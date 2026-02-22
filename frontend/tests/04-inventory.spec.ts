import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Inventory', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/inventory');
    await page.waitForLoadState('networkidle');
  });

  test('shows inventory batches table', async ({ page }) => {
    await expect(page.getByRole('table')).toBeVisible();
    const rows = page.locator('table tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });
  });

  test('opens stock-in dialog', async ({ page }) => {
    await page.getByRole('button', { name: /stock in/i }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await expect(page.getByRole('dialog').getByText(/stock in/i)).toBeVisible();
  });

  test('validates required fields on stock-in', async ({ page }) => {
    await page.getByRole('button', { name: /stock in/i }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByRole('button', { name: /add stock/i }).click();
    // Validation shows a toast warning
    await expect(page.locator('.p-toast-message').first()).toBeVisible({ timeout: 8_000 });
  });

  test('shows expiry badge on expiring batches', async ({ page }) => {
    const expiryBadge = page.locator('.badge-error, .badge-warning, [class*="expir"]').first();
    await expect(expiryBadge).toBeVisible({ timeout: 8_000 });
  });

  test('pagination is present', async ({ page }) => {
    await expect(page.locator('.p-paginator').first()).toBeVisible();
  });
});
