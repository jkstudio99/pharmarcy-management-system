import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Categories', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');
  });

  test('shows categories table with data', async ({ page }) => {
    await expect(page.getByRole('table')).toBeVisible();
    const rows = page.locator('table tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });
  });

  test('opens add category dialog', async ({ page }) => {
    await page.getByRole('button', { name: /add category/i }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
  });

  test('creates a new category', async ({ page }) => {
    const name = `Test Cat ${Date.now()}`;
    await page.getByRole('button', { name: /add category/i }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByPlaceholder(/e\.g\. antibiotics|category name/i).fill(name);
    await dialog.getByRole('button', { name: /create/i }).click();
    await expect(page.getByText(/success/i).first()).toBeVisible({ timeout: 8_000 });
    await expect(page.getByText(name)).toBeVisible({ timeout: 8_000 });
  });

  test('opens edit dialog with pre-filled name', async ({ page }) => {
    await page.locator('table tbody tr').first().getByRole('button').first().click();
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    const input = dialog.getByPlaceholder(/e\.g\. antibiotics|category name/i);
    const value = await input.inputValue();
    expect(value.length).toBeGreaterThan(0);
  });
});
