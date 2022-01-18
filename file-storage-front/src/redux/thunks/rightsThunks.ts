import {createAsyncThunk} from "@reduxjs/toolkit";
import {fetchConfig, fetchData} from "../api/api";


export const fetchAllUsers = createAsyncThunk("users-info/users", async (_, thunkAPI) => {
    try {
        const data = await fetchData("/users");
        return data;
    } catch (e) {
        return thunkAPI.rejectWithValue("Не получилось");
    }
})


export const fetchUserCurrent = createAsyncThunk("users-info/current", async (_, thunkAPI) => {
    try {
        const data = await fetchData("/users/current");
        return data;
    } catch (e) {
        return thunkAPI.rejectWithValue("Не получилось");
    }
})

export const fetchRightsCurrentUser = createAsyncThunk("rights/current", async (_, thunkAPI) => {
    try {
        const data = await fetchData("/rights/currentUserRights");
        return data;
    } catch (e) {
        return thunkAPI.rejectWithValue("Не получилось");
    }
})

export const fetchRightsUserById = createAsyncThunk("rights/user", async (id: string, thunkAPI) => {
    try {
        const data = await fetchData(`/rights/userRights?userId=${id}`);
        return data;
    } catch (e) {
        return thunkAPI.rejectWithValue("Не получилось");
    }
})

export const fetchRightsDescription = createAsyncThunk("rights/description", async (_, thunkAPI) => {
    try {
        return await fetchData("/rights/description");
    } catch (e) {
        return thunkAPI.rejectWithValue("Не войти с помощью гитлаба");
    }
})


export const postSetRightsUser = createAsyncThunk("rights/set", async (args: { userId: string, grant?: Array<string | number>, revoke?: Array<string | number> }, thunkAPI) => {
    try {
        const data = await fetchConfig("/rights/set", {method: "POST", body: args});
        return data;
    } catch (e) {
        return thunkAPI.rejectWithValue("Не войти с помощью гитлаба");
    }
})

