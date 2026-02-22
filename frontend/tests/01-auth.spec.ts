import { test, expect } from '@playwright/test';
import { loginAs, clearSession } from './helpers/auth.helper';

test.describe('Authentication', () => {
  test('redirects unauthenticated user to /login', async ({ page }) => {
    await clearSession(page);
    await page.goto('/dashboard');
    await page.waitForURL(/\/login/, { timeout: 10_000 });
    await expect(page).toHaveURL(/\/login/);
  });

  test('shows validation error on empty submit', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page.getByText(/please enter email and password/i)).toBeVisible({
      timeout: 5_000,
    });
  });

  test('shows error on wrong credentials', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    await page.getByPlaceholder('admin@pharmacy.com').fill('wrong@email.com');
    await page.locator('input[name="password"]').fill('WrongPass123');
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page.locator('.p-message').first()).toBeVisible({ timeout: 10_000 });
  });

  test('logs in successfully as admin and lands on dashboard', async ({ page }) => {
    await loginAs(page);
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.getByText(/dashboard/i).first()).toBeVisible();
  });

  test('logs out and redirects to /login', async ({ page }) => {
    await loginAs(page);
    await page.locator('.logout-btn').click();
    await page.waitForURL(/\/login/, { timeout: 10_000 });
    await expect(page).toHaveURL(/\/login/);
  });
});
