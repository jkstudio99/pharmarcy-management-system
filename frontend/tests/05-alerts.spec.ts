import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Alerts', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/alerts');
    await page.waitForLoadState('networkidle');
  });

  test('shows Low Stock section', async ({ page }) => {
    await expect(page.getByText(/low stock/i).first()).toBeVisible();
  });

  test('shows Expiring Soon section', async ({ page }) => {
    await expect(page.getByText(/expiring soon/i).first()).toBeVisible();
  });

  test('expiry table has rows (13 batches expiring)', async ({ page }) => {
    const rows = page.locator('table tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });
    const count = await rows.count();
    expect(count).toBeGreaterThan(0);
  });

  test('refresh button reloads data', async ({ page }) => {
    await page.getByRole('button', { name: /refresh/i }).click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(/expiring soon/i).first()).toBeVisible();
  });
});
