import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, Observable } from 'rxjs';

import {
  Contact,
  CreateContact,
  UpdateContact,
} from '../../_models/contacts.models';
import { ContactsService } from '../../_services/contacts.service';

@Component({
  selector: 'app-contacts-page',
  imports: [FormsModule],
  templateUrl: './contacts-page.html',
  styleUrl: './contacts-page.scss',
})
export class ContactsPage implements OnInit {
  contacts = signal<Contact[]>([]);
  selectedContact = signal<Contact | null>(null);
  isLoading = signal(false);
  isSubmitting = signal(false);
  isDeleting = signal(false);
  errorMessage = signal('');

  name = '';
  surname = '';
  mobileNumber = '';
  email = '';

  constructor(private readonly contactsService: ContactsService) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.contactsService
      .getAll()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (contacts) => {
          this.contacts.set(contacts);
        },
        error: () => {
          this.errorMessage.set('Unable to load contacts.');
        },
      });
  }

  get isEditMode(): boolean {
    return this.selectedContact() !== null;
  }

  selectContact(contact: Contact): void {
    this.selectedContact.set(contact);
    this.name = contact.name;
    this.surname = contact.surname;
    this.mobileNumber = contact.mobileNumber ?? '';
    this.email = contact.email ?? '';
    this.errorMessage.set('');
  }

  clearSelection(): void {
    this.selectedContact.set(null);
    this.name = '';
    this.surname = '';
    this.mobileNumber = '';
    this.email = '';
    this.errorMessage.set('');
  }

  save(): void {
    if (this.isSubmitting() || this.isDeleting()) {
      return;
    }

    const name = this.name.trim();
    const surname = this.surname.trim();

    if (!name || !surname) {
      this.errorMessage.set('Name and surname are required.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const selectedId = this.selectedContact()?.id;

    let request$ : Observable<Contact> | null = null; 

    if (selectedId == null) {
      // Create new contact
      const payloadCreate : CreateContact = {
        name: name.trim(),
        surname: surname.trim(),
        mobileNumber: this.mobileNumber.trim() || null,
        email: this.email.trim() || null,
      };
      request$ = this.contactsService.create(payloadCreate);
    } else {
      // Update existing contact
      const payloadUpdate : UpdateContact = {
        name,
        surname,
        mobileNumber: this.mobileNumber.trim() || null,
        email: this.email.trim() || null,
        rowVersion: this.selectedContact()?.rowVersion || null,
      };
      request$ = this.contactsService.update(selectedId, payloadUpdate);
    }

    request$
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (savedContact) => {
          const current = this.contacts();
          const index = current.findIndex((contact) => contact.id === savedContact.id);

          if (index >= 0) {
            const updated = [...current];
            updated[index] = savedContact;
            this.contacts.set(updated);
          } else {
            this.contacts.set([savedContact, ...current]);
          }

          this.clearSelection();
        },
        error: () => {
          this.errorMessage.set('Unable to save contact.');
        },
      });
  }

  deleteSelected(): void {
    const selectedId = this.selectedContact()?.id;

    if (!selectedId || this.isDeleting() || this.isSubmitting()) {
      return;
    }

    this.isDeleting.set(true);
    this.errorMessage.set('');

    this.contactsService
      .delete(selectedId)
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          this.contacts.set(this.contacts().filter((contact) => contact.id !== selectedId));
          this.clearSelection();
        },
        error: () => {
          this.errorMessage.set('Unable to delete contact.');
        },
      });
  }
}
