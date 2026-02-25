import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class LoadingService {
    private loadingSubject = new BehaviorSubject<boolean>(false);
    private messageSubject = new BehaviorSubject<string>('Loading, please wait...');

    loading$: Observable<boolean> = this.loadingSubject.asObservable();
    message$: Observable<string> = this.messageSubject.asObservable();

    show(message?: string): void {
        if (message) {
            this.messageSubject.next(message);
        }
        this.loadingSubject.next(true);
    }

    hide(): void {
        this.loadingSubject.next(false);
        // Reset message to default
        this.messageSubject.next('Loading, please wait...');
    }
}
