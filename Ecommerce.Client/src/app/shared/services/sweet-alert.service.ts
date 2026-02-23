import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class SweetAlertService {
  confirm(options: any = {}) {
    const defaultOptions = {
      title: 'Are you sure?',
      text: "You won't be able to revert this!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Yes, proceed!',
      cancelButtonText: 'Cancel',
      reverseButtons: true,
      customClass: {
        popup: 'sweet-alert-popup',
        confirmButton: 'sweet-alert-confirm',
        cancelButton: 'sweet-alert-cancel'
      }
    };

    return Swal.fire({ ...defaultOptions, ...options });
  }

  success(message: string, title: string = 'Success!') {
    return Swal.fire({
      title,
      text: message,
      icon: 'success',
      confirmButtonColor: '#5624d0',
      timer: 2000,
      showConfirmButton: false
    });
  }

  error(message: string, title: string = 'Error!') {
    return Swal.fire({
      title,
      text: message,
      icon: 'error',
      confirmButtonColor: '#d33'
    });
  }

  info(message: string, title: string = 'Info') {
    return Swal.fire({
      title,
      text: message,
      icon: 'info',
      confirmButtonColor: '#5624d0'
    });
  }
}
