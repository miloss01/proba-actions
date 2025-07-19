import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'app/env/environment';

@Injectable({
  providedIn: 'root'
})
export class ImageService {
  private apiUrl = `${environment.apiHost}images`;

  constructor(private http: HttpClient) {}

  getImageUrl(fileName: string): Observable<any> {
    const body = { fileName }; 
    return this.http.put<{ imageUrl: string }>(`${this.apiUrl}/get-image-url`, body);
  }

  uploadImage(filePath: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file); 
    formData.append('filePath', filePath);

    const url = `${this.apiUrl}/upload-image`;
    return this.http.post(url, formData, { responseType: 'text' });
  }

  deleteImage(imagePath: string): Observable<void> {
    const url = `${this.apiUrl}/delete-image?path=${encodeURIComponent(imagePath)}`;
    return this.http.delete<void>(url);
  }

  updateImage(updateDto: { oldFileName: string, newFileName: string }, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('oldFileName', updateDto.oldFileName);
    formData.append('newFileName', updateDto.newFileName);
    formData.append('file', file);
    return this.http.put(`${this.apiUrl}/update`, formData);
  }
}
