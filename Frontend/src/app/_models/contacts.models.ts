export interface Contact {
  id: string;
  code: string;
  name: string;
  surname: string;
  mobileNumber: string | null;
  email: string | null;
  createdBy: string;
  lastModifiedBy: string;
  createdAt: string;
  modifiedAt: string | null;
}

export interface CreateContact {
  name: string;
  surname: string;
  mobileNumber: string | null;
  email: string | null;
}

export interface UpdateContact {
  name: string;
  surname: string;
  mobileNumber: string | null;
  email: string | null;
}
