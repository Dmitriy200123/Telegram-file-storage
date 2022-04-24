import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {ClassificationType} from "../../models/Classification";

export type ClassesModalType = "create" | "edit" | "remove";

type stateType = {
    modal: {
        isOpen: boolean,
        type: null | ClassesModalType,
        args?: any
    },
    count: number,
    classifications: ClassificationType[] | null
}

const initialState: stateType = {
    modal: {
        isOpen: false,
        type: null,
    },
    count: 0,
    classifications: null
}

export const classesDocsSlice = createSlice({
    name: "classesDocs",
    initialState,
    reducers: {
        openModal(state, payload: PayloadAction<{ type: ClassesModalType, args?: any }>) {
            state.modal = {
                isOpen: true,
                type: payload.payload.type,
                args: payload.payload.args
            }
        },
        setCount(state, payload: PayloadAction<number>) {
            state.count = payload.payload;
        },
        setClassifications(state, payload: PayloadAction<ClassificationType[] | null>) {
            state.classifications = payload.payload;
        },
        renameClassification(state, payload: PayloadAction<{id:string, name: string}>) {
            if (!state.classifications)
                return;
            state.classifications = state.classifications.map(classification => {
                if(classification.id === payload.payload.id)
                    return {...classification, name: payload.payload.name};
                return classification;
            });
        },
        deleteClassification(state, payload: PayloadAction<{id:string}>) {
            if (!state.classifications)
                return;
            state.classifications = state.classifications.filter(classification => {
                return classification.id !== payload.payload.id;
            });
        },
        closeModal(state) {
            state.modal = {isOpen: false, type: null, args: null};
        },
    },
    extraReducers: {}
});


export default classesDocsSlice.reducer;
