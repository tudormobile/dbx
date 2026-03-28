import { ref, computed } from 'vue';

export interface DbxResponse<T = any> {
  success: boolean;
  data?: T;
}

export interface DbxStatus {
  version: string;
}

export function useDbx(baseUrl: string = '/api/v1') {
  const loading = ref(false);
  const error = ref<string | null>(null);

  const request = async <T>(path: string, options?: RequestInit): Promise<DbxResponse<T>> => {
    loading.value = true;
    error.value = null;
    try {
      const response = await fetch(`${baseUrl}/${path}`, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          ...options?.headers,
        },
      });

      if (!response.ok && response.status !== 404) {
        throw new Error(`HTTP Error: ${response.status}`);
      }

      const result: DbxResponse<T> = await response.json();
      if (!result.success && response.status !== 404) {
        error.value = (result.data as any)?.message || 'Operation failed';
      }
      return result;
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Unknown error';
      error.value = msg;
      return { success: false, data: { message: msg } as any };
    } finally {
      loading.value = false;
    }
  };

  const getStatus = () => request<DbxStatus>('dbx/status');
  
  const listItems = (id: string) => request<string[]>(`dbx/${id}`);
  
  const createItem = (id: string, data: any) => 
    request<string>(`dbx/${id}`, {
      method: 'POST',
      body: JSON.stringify(data),
    });

  const readItem = (id: string, itemId: string) => 
    request<any>(`dbx/${id}/${itemId}`);

  const updateItem = (id: string, itemId: string, data: any, partial = false) => 
    request<string>(`dbx/${id}/${itemId}`, {
      method: partial ? 'PATCH' : 'PUT',
      body: JSON.stringify(data),
    });

  const deleteItem = (id: string, itemId: string) => 
    request<string>(`dbx/${id}/${itemId}`, {
      method: 'DELETE',
    });

  return {
    loading,
    error,
    getStatus,
    listItems,
    createItem,
    readItem,
    updateItem,
    deleteItem,
  };
}
