import React, {useEffect} from 'react';
import "./FilesMain.scss"
import Paginator from '../utils/Paginator/Paginator';
import FragmentFile from "./FragmentFile";
import {useHistory} from "react-router-dom";
import * as queryString from "querystring";
import {useAppDispatch, useAppSelector} from "../../utils/hooks/reduxHooks";
import {configFilters} from "./ConfigFilters";
import {SubmitHandler, useForm} from "react-hook-form";
import {Select} from "../utils/Inputs/Select";
import {fetchFiles, fetchFilters} from "../../redux/thunks/mainThunks";
import {SelectTime} from "../utils/Inputs/SelectDate";
import {ReactComponent as Search} from "./../../assets/search.svg";

const FilesMain = () => {
    const state = useAppSelector((state) => state);
    const {filesReducer, profile} = state;
    const {rights} = profile;
    const {files:filesData,filesNames, chats, senders, filesCount, paginator} = filesReducer;
    const {currentPage, filesInPage} = paginator;
    const dispatch = useAppDispatch();
    const history = useHistory();

    useEffect(() => {
        const {fileName, chats, senderId, categories, date} = GetQueryParamsFromUrl(history);
        dispatch(fetchFilters());
        setValue("fileName", fileName);
        setValue("senderIds", senderId);
        setValue("categories", categories);
        setValue("chatIds", chats);
        setValue("date", date);
    }, []);

    useEffect(() => {
        onChangeForm();
    },[currentPage])


    const {optionsName, optionsSender, optionsChat, optionsCategory} = configFilters(filesNames, chats, senders);

    const {register, handleSubmit, formState: {errors}, setValue, getValues} = useForm();
    const dispatchValuesForm: SubmitHandler<any> = (formData) => {
        AddToUrlQueryParams(history, formData);
        dispatch(fetchFiles({skip: (currentPage - 1)* filesInPage, take: filesInPage, ...formData}));
    };


    const FragmentsFiles = filesData.map((f) => <FragmentFile key={f.fileId} file={f} rights={rights || []}/>);

    const onChangeForm = handleSubmit(dispatchValuesForm);
    const setValueForm = (name: any, value: any) => {
        setValue(name, value, {
            shouldValidate: true,
            shouldDirty: true
        });
    }
    return (
        <div className={"files-main"}>
            <h2 className={"files-main__title"}>Файлы</h2>
            <div className={"files-main__content"}>
                <form className={"files"} onSubmit={onChangeForm}>
                    <h3 className={"files__title"}>Название</h3>
                    <h3 className={"files__title"}>Дата</h3>
                    <h3 className={"files__title"}>Формат</h3>
                    <h3 className={"files__title"}>Отправитель</h3>
                    <h3 className={"files__title"}>Чаты</h3>
                    <Select name={"fileName"} className={"files__filter files__filter_select"} register={register}
                            onChangeForm={onChangeForm} setValue={setValueForm}
                            values={getValues("fileName")} options={optionsName} isMulti={false}/>
                    <SelectTime name={"date"} className={"files__filter files__filter_select"} register={register}
                                onChangeForm={onChangeForm} setValue={setValueForm}
                                values={getValues("date")} placeholder={"Выберите дату"}/>
                    <Select name={"categories"} className={"files__filter files__filter_select"} register={register}
                            onChangeForm={onChangeForm} setValue={setValueForm}
                            values={getValues("categories")} options={optionsCategory} isMulti={true}/>
                    <Select name={"senderIds"} className={"files__filter files__filter_select"} register={register}
                            onChangeForm={onChangeForm} setValue={setValueForm}
                            values={getValues("senderIds")} options={optionsSender} isMulti={true}/>
                    <div className={"files__filter files__filter_last files__filter_select files__filter_search"} >
                        <Select name={"chatIds"}
                                register={register}
                                onChangeForm={onChangeForm} setValue={setValueForm}
                                values={getValues("chatIds")} options={optionsChat} isMulti={true}/>
                        <button>
                            <Search />
                        </button>
                    </div>
                    {FragmentsFiles}
                </form>
            </div>
            <Paginator paginator={paginator} />
        </div>
    );
};


//#region utils
const AddToUrlQueryParams = (history: any, values: Object) => {
    const urlParams = {};
    Object.keys(values).forEach(key => {
        // @ts-ignore
        const value = values[key];
        if (value) {
            if (value instanceof Array) {
                if (value.length > 0) {
                    // @ts-ignore
                    urlParams[key] = value.join(`&`)
                }
            } else {
                // @ts-ignore
                urlParams[key] = value;
            }
        }
    })

    history.push({
        search: queryString.stringify(urlParams),
    })
};

const GetQueryParamsFromUrl = (history: any) => {
    const urlSearchParams = new URLSearchParams(history.location.search);
    const fileName = urlSearchParams.get("fileName");
    const senderId = urlSearchParams.get("senderIds")?.split("&")?.map((e) => e);
    const categories = urlSearchParams.get("categories")?.split("&")?.map((e) => +e);
    const date = urlSearchParams.get("date");
    const chats = urlSearchParams.get("chatIds")?.split("&")?.map((e) => e);
    return {fileName, senderId, categories, date, chats};
}
//#endregion

export default FilesMain;
