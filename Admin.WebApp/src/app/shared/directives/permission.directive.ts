// src/app/shared/directives/permission.directive.ts
import {
    Directive,
    Input,
    OnInit,
    TemplateRef,
    ViewContainerRef,
    OnDestroy
} from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

@Directive({
    selector: '[appHasPermission]',
    standalone: true
})
export class HasPermissionDirective implements OnInit, OnDestroy {
    @Input('appHasPermission') permission!: string;
    @Input('appHasPermissionElse') elseTemplateRef?: TemplateRef<any>;

    private destroy$ = new Subject<void>();
    private hasView = false;

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private authService: AuthService
    ) { }

    ngOnInit() {
        this.authService.hasPermission(this.permission)
            .pipe(takeUntil(this.destroy$))
            .subscribe(hasPermission => {
                if (hasPermission && !this.hasView) {
                    this.viewContainer.clear();
                    this.viewContainer.createEmbeddedView(this.templateRef);
                    this.hasView = true;
                } else if (!hasPermission && this.hasView) {
                    this.viewContainer.clear();
                    if (this.elseTemplateRef) {
                        this.viewContainer.createEmbeddedView(this.elseTemplateRef);
                    }
                    this.hasView = false;
                }
            });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
}

@Directive({
    selector: '[appHasRole]',
    standalone: true
})
export class HasRoleDirective implements OnInit, OnDestroy {
    @Input('appHasRole') role!: string;
    @Input('appHasRoleElse') elseTemplateRef?: TemplateRef<any>;

    private destroy$ = new Subject<void>();
    private hasView = false;

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private authService: AuthService
    ) { }

    ngOnInit() {
        this.authService.hasRole(this.role)
            .pipe(takeUntil(this.destroy$))
            .subscribe(hasRole => {
                if (hasRole && !this.hasView) {
                    this.viewContainer.clear();
                    this.viewContainer.createEmbeddedView(this.templateRef);
                    this.hasView = true;
                } else if (!hasRole && this.hasView) {
                    this.viewContainer.clear();
                    if (this.elseTemplateRef) {
                        this.viewContainer.createEmbeddedView(this.elseTemplateRef);
                    }
                    this.hasView = false;
                }
            });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
}