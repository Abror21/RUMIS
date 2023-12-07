import {Button, Dropdown, Form, Input, MenuProps, Row, Space, Spin, Tabs} from "antd"
import {useForm} from "antd/es/form/Form"
import React, {useEffect, useMemo, useState} from "react"
import {initialValues} from "./resources"
import {ResourceFilterType} from "@/app/types/Resource"
import useQueryApiClient from "@/app/utils/useQueryApiClient"
import SearchSelectInput from "@/app/components/searchSelectInput"
import {ClassifierResponse} from "@/app/types/Classifiers"
import {EducationalInstitution} from "@/app/types/EducationalInstitution"
import {SelectOption} from "@/app/types/Antd"
import {SettingOutlined} from "@ant-design/icons";
import type {TabsProps} from 'antd';

type ResourceFiltersProps = {
    filtersLoading: boolean;
    activeFilters: ResourceFilterType,
    filterState: (val: ResourceFilterType) => void
    refresh: (val: ResourceFilterType) => void,
    userFilterOptions: any[],
    setChangeFiltersModalIsOpen: (value: boolean) => void,
    defaultResourceStatusIds?: string[] | null
    defaultSupervisorIds?: string | null,
    defaultUsagePurposeTypeIds?: string | null,
}

const ResourceFilters = ({filtersLoading, activeFilters, filterState, refresh, userFilterOptions, setChangeFiltersModalIsOpen, defaultSupervisorIds, defaultResourceStatusIds, defaultUsagePurposeTypeIds}: ResourceFiltersProps) => {
    const [filterList, setFilterList] = useState<any[]>([])
    const [allSubTypes, setAllSubTypes] = useState<ClassifierResponse[]>([])
    const [filteredSubTypes, setFilteredSubTypes] = useState<ClassifierResponse[]>([])
    const [allEducationalInstitution, setAllEducationalInstitution] = useState<EducationalInstitution[]>([])
    const [filteredEducationalInstitution, setFilteredEducationalInstitution] = useState<EducationalInstitution[]>([])
    const [supervisorOptions, setSupervisorOptions] = useState<SelectOption[]>([])

    const [form] = useForm()

    const [showTabs, setShowTabs] = useState<boolean>(false);
    const [activeTabKey, setActiveTabKey] = useState<string>('userSettings');
    const resourceType = Form.useWatch('resourceType', form)
    const resourceSubTypeIds = Form.useWatch('resourceSubTypeIds', form)
    const supervisor = Form.useWatch('supervisor', form)
    const educationalInstitutionIds = Form.useWatch('educationalInstitutionIds', form)

    useEffect(() => {
        if(!filtersLoading){
            if (userFilterOptions.length > 0) {
                onReset();
                setShowTabs(true);
                setActiveTabKey('userSettings');
                filterListHandle(userFilterOptions);
            }
            else {
                onReset();
                filterListHandle(allFilterOptions);
            }
        }
    }, [userFilterOptions])

    useEffect(() => {
        if (resourceType) {
            // @ts-ignore
            const filteredValues = allSubTypes.filter(type => type.payload.resource_type === resourceType)
            setFilteredSubTypes(filteredValues)

            if (!filteredValues.find(v => v.id === resourceSubTypeIds)) {
                form.setFieldValue('resourceSubTypeIds', null)
            }
        }
    }, [resourceType])

    useEffect(() => {
        if (supervisor) {
            const filteredValues = allEducationalInstitution.filter(edu => edu.supervisor.id === supervisor)
            setFilteredEducationalInstitution(filteredValues)

            if (!filteredValues.find(v => v.id === educationalInstitutionIds)) {
                form.setFieldValue('educationalInstitutionIds', null)
            }
        }
    }, [supervisor])

    useEffect(() => {
        if(!filtersLoading){
            activeTabKey === 'userSettings' && userFilterOptions.length > 0 ? filterListHandle(userFilterOptions) : filterListHandle(allFilterOptions);
        }
    }, [filteredSubTypes, filteredEducationalInstitution])

    const {
        data: manufacterOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_manufacturer',
                includeDisabled: false
            }
        },
    })

    const {
        data: resourceStatusOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_status',
                includeDisabled: false
            }
        },
    })

    const {
        data: resourceTypeOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_type',
                includeDisabled: false
            }
        },
    })

    const {
        data: resourceGroupOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_group',
                includeDisabled: false
            }
        },
    })

    const {
        data: resourceUsingPurposeOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_using_purpose',
                includeDisabled: false
            }
        },
    })

    const {
        data: targetGroupOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'target_group',
                includeDisabled: false
            }
        },
    })

    const {
        data: resourceLocationOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_location',
                includeDisabled: false
            }
        },
    })

    const {
        data: modelOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_model_name',
                includeDisabled: false
            }
        },
    })

    const {
        data: resourceAcquisitionTypeOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_acquisition_type',
                includeDisabled: false
            }
        },
    })

    const {
        data: subTypeOptions
    } = useQueryApiClient({
        request: {
            url: `/classifiers`,
            data: {
                type: 'resource_subtype',
                includeDisabled: false
            }

        },
        onSuccess: (response: ClassifierResponse[]) => {
            const parsedResponse = response.map(item => {
                // @ts-ignore
                const payloadObj = JSON.parse(item.payload);
                return {
                    ...item,
                    payload: payloadObj
                };
            });
            setAllSubTypes(parsedResponse)
            setFilteredSubTypes(parsedResponse)
        }
    });

    const {} = useQueryApiClient({
        request: {
            url: `/educationalInstitutions`,
        },
        onSuccess: (response: EducationalInstitution[]) => {
            setAllEducationalInstitution(response)
            setFilteredEducationalInstitution(response)

            const uniqueSupervisors = new Set();
            const supervisorOptions: SelectOption[] = []

            response.forEach(edu => {
                if (!uniqueSupervisors.has(edu.supervisor.id)) {
                    uniqueSupervisors.add(edu.supervisor.id);

                    supervisorOptions.push({
                        value: edu.supervisor.id,
                        label: edu.supervisor.name,
                        rest: edu.supervisor.code
                    })
                }
            });
            setSupervisorOptions(supervisorOptions)
        }
    });

    const allFilterOptions: any[] = [
        {
            name: 'Resursa kods',
            key: 'resourceNumber',
            render: () => (
                <Form.Item name="resourceNumber" label="Resursa kods">
                    <Input placeholder="JVSK-LEN0068"/>
                </Form.Item>)
        },
        {
            name: 'Inventāra Nr.',
            key: 'inventoryNumber',
            render: () => (<Form.Item name="inventoryNumber" label="Inventāra Nr.">
                <Input placeholder="AS-PF0-2"/>
            </Form.Item>)
        },
        {
            name: 'Sērijas Nr.',
            key: 'serialNumber',
            render: () => (<Form.Item name="serialNumber" label="Sērijas Nr.">
                <Input placeholder="PF0F89B7"/>
            </Form.Item>)
        },
        {
            name: 'Resursa nosaukums',
            key: 'modelNameIds',
            render: () => (<Form.Item name="modelNameIds" label="Resursa nosaukums">
                <SearchSelectInput 
                    placeholder="Asus Chromebook Flip CX3 CX3400FMA-EC0226 1"
                    options={modelOptions.map((status: ClassifierResponse) => ({
                        value: status.id,
                        label: status.value,
                    }))}
                    mode="multiple"
                />
            </Form.Item>)
        },
        {
            name: 'Iestādes piešķirtais nosaukums',
            key: 'resourceName',
            render: () => (<Form.Item name="resourceName" label="Iestādes piešķirtais nosaukums">
                <Input placeholder="Chromebook dators"/>
            </Form.Item>)
        },
        {
            name: 'Modelis',
            key: 'modelIdentifier',
            render: () => (<Form.Item name="modelIdentifier" label="Modelis">
                <Input />
            </Form.Item>)
        },
        {
            name: 'Ražotājs',
            key: 'manufacturer',
            render: () => (<Form.Item name="manufacturer" label="Ražotājs">
                <SearchSelectInput placeholder="Izvēlies no saraksta"
                                   options={manufacterOptions.map((manufacter: ClassifierResponse) => ({
                                       value: manufacter.id,
                                       label: manufacter.value,
                                   }))}/>
            </Form.Item>)
        },
        {
            name: 'Resursa statuss',
            key: 'resourceStatusIds',
            render: () => (<Form.Item name="resourceStatusIds" label="Resursa statuss">
                <SearchSelectInput 
                    placeholder="Izvēlies no saraksta"
                    options={resourceStatusOptions.map((status: ClassifierResponse) => ({
                        value: status.id,
                        label: status.value,
                    }))}
                    mode="multiple"
                />
            </Form.Item>)
        },
        {
            name: 'Resursa veids',
            key: 'resourceType',
            render: () => (<Form.Item name="resourceType" label="Resursa veids">
                <SearchSelectInput placeholder="Izvēlies no saraksta"
                                   options={resourceTypeOptions.map((option: ClassifierResponse) => ({
                                       value: option.code,
                                       label: option.value,
                                   }))}/>
            </Form.Item>)
        },
        {
            name: 'Resursa paveids',
            key: 'resourceSubTypeIds',
            render: () => (<Form.Item name="resourceSubTypeIds" label="Resursa paveids">
                <SearchSelectInput placeholder="Izvēlies no saraksta"
                                   options={filteredSubTypes.map((option: ClassifierResponse) => ({
                                       value: option.id,
                                       label: option.value,
                                   }))}/>
            </Form.Item>)
        },
        {
            name: 'Resursa grupa',
            key: 'resourceGroup',
            render: () => (<Form.Item name="resourceGroup" label="Resursa grupa">
                <SearchSelectInput placeholder="Izvēlies no saraksta"
                                   options={resourceGroupOptions.map((option: ClassifierResponse) => ({
                                       value: option.id,
                                       label: option.value,
                                   }))}/>
            </Form.Item>)
        },
        {
            name: 'Izmantošanas mērķis',
            key: 'usagePurposeTypeIds',
            render: () => (<Form.Item name="usagePurposeTypeIds" label="Izmantošanas mērķis">
                <SearchSelectInput 
                    placeholder="Izvēlies no saraksta"
                    options={resourceUsingPurposeOptions.map((option: ClassifierResponse) => ({
                        value: option.id,
                        label: option.value,
                    }))}
                    mode="multiple"
                />
            </Form.Item>)
        },
        {
            name: 'Mērķa grupa',
            key: 'targetGroupIds',
            render: () => (<Form.Item name="targetGroupIds" label="Mērķa grupa">
                <SearchSelectInput 
                    placeholder="Izvēlies no saraksta"
                    options={targetGroupOptions.map((option: ClassifierResponse) => ({
                        value: option.id,
                        label: option.value,
                    }))}
                    mode="multiple"
                />
            </Form.Item>)
        },
        {
            name: 'Atrašanās vieta',
            key: 'resourceLocationIds',
            render: () => (<Form.Item name="resourceLocationIds" label="Atrašanās vieta">
                <SearchSelectInput 
                    placeholder="Izvēlies no saraksta"
                    options={resourceLocationOptions.map((option: ClassifierResponse) => ({
                        value: option.id,
                        label: option.value,
                    }))}
                    mode="multiple"
                />
            </Form.Item>)
        },
        {
            name: 'Vadošā iestāde',
            key: 'supervisorIds',
            render: () => (<Form.Item name="supervisorIds" label="Vadošā iestāde">
                <SearchSelectInput placeholder="Izvēlies no saraksta" options={supervisorOptions}// @ts-ignore
                filterOption={(input: string, option?: { label: string; value: string, rest: string }) => {
                  const restMatch = (option?.rest ?? '').toLowerCase().includes(input.toLowerCase());
                  const labelMatch = (option?.label ?? '').toLowerCase().includes(input.toLowerCase());

                  return restMatch || labelMatch;
                }}
              />
            </Form.Item>)
        },
        {
            name: 'Izglītības iestāde',
            key: 'educationalInstitutionIds',
            render: () => (<Form.Item name="educationalInstitutionIds" label="Izglītības iestāde">
                <SearchSelectInput placeholder="Izvēlies no saraksta"
                                   options={filteredEducationalInstitution.map(item => ({
                                       value: item.id,
                                       label: item.name,
                                   rest: item.code
                }))}
                // @ts-ignore
                filterOption={(input: string, option?: { label: string; value: string, rest: string }) => {
                  const restMatch = (option?.rest ?? '').toLowerCase().includes(input.toLowerCase());
                  const labelMatch = (option?.label ?? '').toLowerCase().includes(input.toLowerCase());

                  return restMatch || labelMatch;
                }}/>
            </Form.Item>)
        },
        {
            name: 'Iegādes veids',
            key: 'acquisitionTypeIds',
            render: () => (<Form.Item name="acquisitionTypeIds" label="Iegādes veids">
                <SearchSelectInput placeholder="Izvēlies no saraksta" options={resourceAcquisitionTypeOptions.map((option: ClassifierResponse) => ({
                    value: option.id,
                    label: option.value,
                }))}
              />
            </Form.Item>)
        },
    ]

    const filterListHandle = (filterOptions: any[]) => {
        const newFilterOptions: any[] = []
        filterOptions.map(filterOption => {
            if (allFilterOptions.find(o => o.key === filterOption.key)) {
                newFilterOptions.push(allFilterOptions.find(o => o.key === filterOption.key))
            }
        })
        setFilterList(newFilterOptions)
    }

    const onFinish = (values: any) => {
        const filters: ResourceFilterType = {
            ...activeFilters,
            ...initialValues,
            resourceNumber: values.resourceNumber,
            inventoryNumber: values.inventoryNumber,
            serialNumber: values.serialNumber,
            resourceName: values.resourceName,
            modelNameIds: values.modelNameIds,
            manufacturer: values.manufacturer,
            resourceStatusIds: values.resourceStatusIds,
            resourceSubTypeIds: values.resourceSubTypeIds,
            resourceGroup: values.resourceGroup,
            usagePurposeTypeIds: values.usagePurposeTypeIds,
            targetGroupIds: values.targetGroupIds,
            resourceLocationIds: values.resourceLocationIds,
            supervisorIds: values.supervisorIds,
            educationalInstitutionIds: values.educationalInstitutionIds,
            modelIdentifier: values.modelIdentifier,
            acquisitionTypeIds: values.acquisitionTypeIds,
        }

        filterState(filters)
        refresh(filters)
    }

    const onReset = () => {
        form.resetFields()

        filterState({
            ...initialValues,
            take: activeFilters.take,
        })
        refresh({
            ...initialValues,
            take: activeFilters.take,
        })
    }

    const tabItems: TabsProps['items'] = [
        {
            key: 'userSettings',
            label: 'Mans saglabātais filtrs',
            children: '',
        },
        {
            key: 'defaultSettings',
            label: 'Noklusējuma meklētājs',
            children: '',
        }
    ]

    const onTabChange = (key: string) => {
        onReset();
        setActiveTabKey(key);
        key == 'userSettings' ? filterListHandle(userFilterOptions) : filterListHandle(allFilterOptions)
    };

    const initialData = useMemo(() => {
        return {
            supervisorIds: defaultSupervisorIds ? +defaultSupervisorIds : null,
            resourceStatusIds: defaultResourceStatusIds ?? null,
            usagePurposeTypeIds: defaultUsagePurposeTypeIds ? [defaultUsagePurposeTypeIds] : []
        }
    }, [])
    return (
        <div className="bg-white rounded-lg p-6">
            {showTabs && <Tabs activeKey={activeTabKey} items={tabItems} onChange={onTabChange} />}
            <Spin spinning={filtersLoading}>
                <Form
                    form={form}
                    name="resourcsFilters"
                    onFinish={onFinish}
                    layout="vertical"
                    initialValues={initialData}
                >
                    <div className="mt-4">
                        <div className="grid grid-cols-2 md:grid-cols-4 xl:grid-cols-6 gap-4 items-end">
                            {filterList.map(filter => (
                                <React.Fragment key={filter.key}>
                                    {filter.render()}
                                </React.Fragment>
                            ))}
                        </div>
                            <Row style={{marginBottom: 0}}>
                                <div className='flex gap-2'>
                                    <Button type="primary" htmlType="submit">
                                        Atlasīt
                                    </Button>
                                    <Button htmlType="button" onClick={onReset}>
                                        Notīrīt
                                    </Button>
                                </div>
                                <div className="ml-auto">
                                    <Button onClick={() => setChangeFiltersModalIsOpen(true)}>
                                        <SettingOutlined/>
                                        Konfigurēt
                                    </Button>
                                </div>
                            </Row>
                    </div>
                </Form>
            </Spin>
        </div>
    )
}

export default ResourceFilters