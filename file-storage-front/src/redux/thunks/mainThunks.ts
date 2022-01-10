import {createAsyncThunk} from "@reduxjs/toolkit";
import {Category, Chat} from "../../models/File";
import {fetchData, fetchConfig, fetchConfigText} from "../api/api";

export const fetchChats = createAsyncThunk("files/chats", async (_, thunkAPI) => {
    try {
        const data: Array<Chat> = await fetchData("/api/chats");
        return data;
    } catch (e) {
        return thunkAPI.rejectWithValue("Не удалось загрузить чаты");
    }
})

export const fetchFilters = createAsyncThunk("files/filters", async (_, thunkAPI) => {
    try {
        const chats = fetchData("/api/chats");
        const senders = fetchData("/api/senders");
        const filesNames = fetchData("/api/files/names");
        const countFiles = fetchConfigText("/api/files/count");
        return {chats: await chats, senders: await senders, countFiles: await countFiles, filesNames: await filesNames};
    } catch (err) {
        return thunkAPI.rejectWithValue("Не удалось загрузить фильтры");
    }
})

type TypeFilesFetchFilters = {
    skip: number,
    take: number,
    categories?: Category
}

export const fetchFiles = createAsyncThunk("files/files", async (args: TypeFilesFetchFilters, thunkAPI) => {
    try {
        return await fetchConfig(`/api/files`, {params: args});
    } catch (err) {
        return thunkAPI.rejectWithValue("Не удалось загрузить файлы");
    }
})

