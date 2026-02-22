import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Sales', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/sales');
    await page.waitForLoadState('networkidle');
  });

  test('shows sales orders table', async ({ page }) => {
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('opens new order dialog', async ({ page }) => {
    await page.getByRole('button', { name: /new order/i }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await expect(page.getByText('New Sales Order')).toBeVisible();
  });

  test('can add and remove order items', async ({ page }) => {
    await page.getByRole('button', { name: /new order/i }).click();
    const dialog = page.getByRole('dialog');
    // Start with 1 item row, click Add Item to get 2
    await dialog.getByRole('button', { name: /add item/i }).click();
    const itemRows = dialog.locator('.item-row');
    await expect(itemRows).toHaveCount(2, { timeout: 5_000 });
    // Remove last item
    await itemRows.last().locator('button').last().click();
    await expect(itemRows).toHaveCount(1, { timeout: 5_000 });
  });

  test('shows order detail on view click', async ({ page }) => {
    const rows = page.locator('table tbody tr');
    const count = await rows.count();
    if (count > 0) {
      await rows.first().locator('button').click();
      await expect(page.getByRole('dialog')).toBeVisible({ timeout: 8_000 });
      await expect(page.getByText('Order Details')).toBeVisible();
    }
  });
});
