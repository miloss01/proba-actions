import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'app/env/environment';
import { DockerImageDTO, PageDTO } from 'app/models/models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DockerImageService {

  constructor(private http: HttpClient) { }

  getDockerImages(page: number, pageSize: number, searchTerm: string, badges: string): Observable<PageDTO<DockerImageDTO>>{
    return this.http.get<PageDTO<DockerImageDTO>>(`${environment.apiHost}dockerImages?page=${page}&pageSize=${pageSize}&searchTerm=${searchTerm}&badges=${badges}`);
  }

  deleteDockerImage(id: string): Observable<void>{
    return this.http.delete<void>(`${environment.apiHost}dockerImages/delete/${id}`);
  }
}
