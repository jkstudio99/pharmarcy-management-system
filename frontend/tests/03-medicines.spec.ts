import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Medicines', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
    await page.goto('/medicines');
    await page.waitForLoadState('networkidle');
  });

  test('shows medicines table with data', async ({ page }) => {
    await expect(page.getByRole('table')).toBeVisible();
    const rows = page.locator('table tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });
  });

  test('search filters medicines', async ({ page }) => {
    const searchInput = page.getByPlaceholder(/search/i);
    await searchInput.fill('Paracetamol');
    await searchInput.press('Enter');
    await page.waitForLoadState('networkidle');
    const rows = page.locator('table tbody tr');
    const count = await rows.count();
    expect(count).toBeGreaterThan(0);
    await expect(rows.first()).toContainText(/paracetamol/i);
  });

  test('opens create medicine dialog', async ({ page }) => {
    await page.getByRole('button', { name: /add medicine/i }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await expect(page.getByRole('dialog').locator('input').first()).toBeVisible();
  });

  test('validates required fields on create', async ({ page }) => {
    await page.getByRole('button', { name: /add medicine/i }).click();
    await page
      .getByRole('dialog')
      .getByRole('button', { name: /create/i })
      .click();
    await expect(page.getByText(/drug name is required/i)).toBeVisible({ timeout: 5_000 });
  });

  test('creates a new medicine', async ({ page }) => {
    const drugName = `Test Drug ${Date.now()}`;
    await page.getByRole('button', { name: /add medicine/i }).click();
    const dialog = page.getByRole('dialog');
    await dialog.locator('input').first().fill(drugName);
    await dialog.getByRole('button', { name: /create/i }).click();
    await expect(page.getByText(/success/i).first()).toBeVisible({ timeout: 8_000 });
  });

  test('opens edit dialog with pre-filled data', async ({ page }) => {
    await page.locator('table tbody tr').first().locator('button').nth(0).click();
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    const firstInput = dialog.locator('input').first();
    const value = await firstInput.inputValue();
    expect(value.length).toBeGreaterThan(0);
  });

  test('pagination controls are visible when records > pageSize', async ({ page }) => {
    await expect(page.locator('.p-paginator').first()).toBeVisible();
  });
});
