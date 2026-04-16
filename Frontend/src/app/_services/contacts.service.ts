import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  Contact,
  CreateContact,
  UpdateContact,
} from '../_models/contacts.models';

@Injectable({
  providedIn: 'root',
})
export class ContactsService {
  private readonly contactsUrl = `${environment.apiBaseUrl}/contacts`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Contact[]> {
    return this.http.get<Contact[]>(this.contactsUrl);
  }

  getById(id: string): Observable<Contact> {
    return this.http.get<Contact>(`${this.contactsUrl}/${id}`);
  }

  create(dto: CreateContact): Observable<Contact> {
    return this.http.post<Contact>(this.contactsUrl, dto);
  }

  update(id: string, dto: UpdateContact): Observable<Contact> {
    return this.http.put<Contact>(`${this.contactsUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.contactsUrl}/${id}`);
  }
}
