import { Component, signal, computed, HostListener, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { InventoryService } from '../../core/services/inventory.service';
import { ThemeService } from '../../core/services/theme.service';
import { I18nService, LANGUAGES } from '../../core/services/i18n.service';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { BadgeModule } from 'primeng/badge';
import { SelectButtonModule } from 'primeng/selectbutton';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  adminOnly?: boolean;
  pharmacistUp?: boolean;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ButtonModule,
    ToastModule,
    BadgeModule,
    SelectButtonModule,
    SelectModule,
    TooltipModule,
    TranslateModule,
  ],
  providers: [MessageService],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent implements OnInit {
  sidebarOpen = signal(false);
  sidebarCollapsed = signal(false);

  readonly languages = LANGUAGES;

  readonly navItems: NavItem[] = [
    { label: 'NAV.DASHBOARD', icon: 'pi pi-th-large', route: '/dashboard' },
    { label: 'NAV.MEDICINES', icon: 'pi pi-box', route: '/medicines', pharmacistUp: false },
    { label: 'NAV.INVENTORY', icon: 'pi pi-database', route: '/inventory' },
    { label: 'NAV.SALES', icon: 'pi pi-shopping-cart', route: '/sales', pharmacistUp: true },
    { label: 'NAV.ALERTS', icon: 'pi pi-bell', route: '/alerts' },
    { label: 'NAV.CATEGORIES', icon: 'pi pi-tag', route: '/categories', pharmacistUp: true },
    { label: 'NAV.SUPPLIERS', icon: 'pi pi-truck', route: '/suppliers', adminOnly: true },
    { label: 'NAV.EMPLOYEES', icon: 'pi pi-users', route: '/employees', adminOnly: true },
    { label: 'NAV.TRANSACTIONS', icon: 'pi pi-arrow-right-arrow-left', route: '/transactions' },
    { label: 'NAV.AUDIT_LOG', icon: 'pi pi-shield', route: '/audit-log', adminOnly: true },
  ];

  visibleNavItems = computed(() => {
    const isAdmin = this.auth.isAdmin();
    const isPharmacistUp = this.auth.isPharmacistUp();
    return this.navItems.filter((item) => {
      if (item.adminOnly) return isAdmin;
      if (item.pharmacistUp) return isPharmacistUp;
      return true;
    });
  });

  user = computed(() => this.auth.user());
  alertCount = signal(0);

  constructor(
    readonly auth: AuthService,
    readonly theme: ThemeService,
    readonly i18n: I18nService,
    private inventoryService: InventoryService,
  ) {}

  ngOnInit() {
    this.loadAlertCount();
  }

  loadAlertCount() {
    this.inventoryService.getAlerts().subscribe({
      next: (res) => {
        if (res.success) {
          const total = res.data.lowStockAlerts.length + res.data.expiryAlerts.length;
          this.alertCount.set(total);
        }
      },
    });
  }

  @HostListener('document:keydown.escape')
  closeSidebar() {
    this.sidebarOpen.set(false);
  }

  toggleSidebar() {
    this.sidebarOpen.update((v) => !v);
  }

  toggleCollapse() {
    this.sidebarCollapsed.update((v) => !v);
  }

  logout() {
    this.auth.logout();
  }
}
